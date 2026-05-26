using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class TransferOrderFinishedConsumer(
    IRepository<PickingNote> pickingNoteRepository,
    IRepository<InventoryItem> inventoryRepository,
    InventoryService inventoryService,
    IUnitOfWork unitOfWork,
    ILogger<TransferOrderFinishedConsumer> logger
) : IConsumer<TransferOrderFinishedEvent>
{
    public async Task Consume(ConsumeContext<TransferOrderFinishedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Warehouse received TransferOrderFinishedEvent: {TransferOrderId}", message.TransferOrderId);

        var pickingNote = await pickingNoteRepository.GetByIdAsync(new PickingNoteId(message.PickingNoteId), context.CancellationToken);
        if (pickingNote != null)
        {
            pickingNote.ActivateFromTransfer();
            
            var productIds = pickingNote.LineItems.Select(x => x.ProductId).ToList();
            var spec = new GetInventoryItemByProductIds(new WarehouseId(message.ToWarehouseId), productIds);
            var inventory = await inventoryRepository.ListAsync(spec, context.CancellationToken);
            
            inventoryService.StartPicking(inventory, pickingNote);
            
            await pickingNoteRepository.Update(pickingNote, context.CancellationToken);
            await inventoryRepository.UpdateRange(inventory, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            
            logger.LogInformation("Activated PickingNote {PickingNoteId} from TransferOrder", message.PickingNoteId);
        }
    }
}
