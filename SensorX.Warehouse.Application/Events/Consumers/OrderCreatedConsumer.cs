
using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class OrderCreatedConsumer(
    ILogger<OrderCreatedConsumer> logger,
    IRepository<PickingNote> pickingNoteRepository,
    IUnitOfWork unitOfWork) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received OrderCreatedEvent for OrderId: {OrderId}, OrderCode: {OrderCode}", message.OrderId, message.OrderCode);

        // Logic to create PickingNote from Order
        // For now, just logging. Will need Order details from Master project.
        // Placeholder: Create a PickingNote with dummy data for now.
        var orderId = new OrderId(message.OrderId);
        var noteCode = Code.Create($"PN-{message.OrderCode}");
        var description = $"Picking Note for Order {message.OrderCode}";
        // Use DeliveryInfo from event
        var deliveryInfo = new DeliveryInfo(
            message.ReceiverName,
            message.ReceiverPhone,
            message.DeliveryAddress,
            message.CompanyName,
            message.TaxCode
        );

        var pickingNote = PickingNote.CreateForSalesOrder(new WarehouseId(message.WarehouseId), orderId, noteCode, description, deliveryInfo);

        await pickingNoteRepository.Add(pickingNote, context.CancellationToken);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Created PickingNote {PickingNoteCode} for Order {OrderCode}", pickingNote.Code.Value, message.OrderCode);
    }
}
