using MediatR;
using MassTransit;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class TransferOrderCreatedConsumer(
    IRepository<PickingNote> pickingNoteRepository,
    ILogger<TransferOrderCreatedConsumer> logger
) : IConsumer<TransferOrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<TransferOrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Warehouse received TransferOrderCreatedEvent: {TransferOrderId}", message.TransferOrderId);

        var sourceId = new TransferOrderId(message.TransferOrderId);
        var code = Code.From(message.TransferOrderCode);
        var description = $"Picking Note for Transfer Order {message.TransferOrderCode}";

        // Placeholder for DeliveryInfo - cần làm rõ logic lấy thông tin này
        var deliveryInfo = new DeliveryInfo("Transfer", "N/A", "N/A", "N/A", "N/A");

        var pickingNote = PickingNote.CreateForTransferOrder(new WarehouseId(message.FromWarehouseId), sourceId, code, description, deliveryInfo);

        await pickingNoteRepository.Add(pickingNote, context.CancellationToken);
        logger.LogInformation("Created PickingNote for TransferOrder: {TransferOrderId}", message.TransferOrderId);
    }
}