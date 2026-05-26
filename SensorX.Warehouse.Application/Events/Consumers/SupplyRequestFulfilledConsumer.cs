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

public class SupplyRequestFulfilledConsumer(
    IRepository<PickingNote> pickingNoteRepository,
    IRepository<InventoryItem> inventoryRepository,
    InventoryService inventoryService,
    IUnitOfWork unitOfWork,
    ILogger<SupplyRequestFulfilledConsumer> logger
) : IConsumer<SupplyRequestFulfilledEvent>
{
    public async Task Consume(ConsumeContext<SupplyRequestFulfilledEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Warehouse received SupplyRequestFulfilledEvent: {SupplyRequestId}", message.SupplyRequestId);

        var pickingNote = await pickingNoteRepository.GetByIdAsync(new PickingNoteId(message.PickingNoteId), context.CancellationToken);
        if (pickingNote != null)
        {
            pickingNote.ActivateFromSupply();
            
            var productIds = pickingNote.LineItems.Select(x => x.ProductId).ToList();
            var spec = new GetInventoryItemByProductIds(new WarehouseId(message.WarehouseId), productIds);
            var inventory = await inventoryRepository.ListAsync(spec, context.CancellationToken);
            
            inventoryService.StartPicking(inventory, pickingNote);
            
            await pickingNoteRepository.Update(pickingNote, context.CancellationToken);
            await inventoryRepository.UpdateRange(inventory, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            
            logger.LogInformation("Activated PickingNote {PickingNoteId} from SupplyRequest", message.PickingNoteId);
        }
    }
}
