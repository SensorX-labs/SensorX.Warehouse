using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class TransferOrderCreatedConsumer(
    IRepository<PickingNote> pickingNoteRepository,
    IUnitOfWork unitOfWork,
    ILogger<TransferOrderCreatedConsumer> logger
) : IConsumer<TransferOrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<TransferOrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Warehouse received TransferOrderCreatedEvent: {TransferOrderId}", message.TransferOrderId);

        // 1. Link to destination picking note if any
        if (message.PickingNoteId != Guid.Empty)
        {
            var destNote = await pickingNoteRepository.GetByIdAsync(new PickingNoteId(message.PickingNoteId), context.CancellationToken);
            if (destNote != null)
            {
                destNote.SetLinkedTransferOrder(message.TransferOrderId);
                await pickingNoteRepository.Update(destNote, context.CancellationToken);
                logger.LogInformation("Linked TransferOrder {TransferOrderId} to destination PickingNote {PickingNoteId}", message.TransferOrderId, message.PickingNoteId);
            }
        }

        // 2. Create source picking note for the FromWarehouse so they can pick and ship it
        if (message.FromWarehouseId != Guid.Empty && message.Items.Any())
        {
            var noteCode = Code.Create("PN-TO"); // Or standard generation
            var sourceNote = PickingNote.CreateForTransferOrder(
                new WarehouseId(message.FromWarehouseId),
                new TransferOrderId(message.TransferOrderId),
                noteCode,
                $"Pick items for Transfer Order {message.TransferOrderCode}",
                new DeliveryInfo("", Code.From(""), "", "", "") // Dummy delivery info for TO
            );

            foreach (var item in message.Items)
            {
                sourceNote.AddItem(
                    new ProductId(item.ProductId),
                    Code.From(item.ProductCode),
                    item.ProductName,
                    item.Unit,
                    new Quantity((int)item.Quantity),
                    item.Manufacturer,
                    item.Note
                );
            }

            await pickingNoteRepository.Add(sourceNote, context.CancellationToken);
            logger.LogInformation("Created source PickingNote {PickingNoteId} for FromWarehouse {FromWarehouseId}", sourceNote.Id.Value, message.FromWarehouseId);
        }

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}