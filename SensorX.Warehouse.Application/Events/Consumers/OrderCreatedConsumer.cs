using MassTransit;
using Microsoft.Extensions.Configuration;
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
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("OrderCreatedConsumer: [START] Consumed OrderCreatedEvent for OrderId={OrderId}, OrderCode={OrderCode}, NearestWarehouseId={NearestWarehouseId}", 
            message.OrderId, message.OrderCode, message.NearestWarehouseId);
        
        var localWarehouseIdStr = configuration["WAREHOUSE_ID"] ?? configuration["Warehouse:Id"];
        if (Guid.TryParse(localWarehouseIdStr, out var localWarehouseGuid))
        {
            if (message.NearestWarehouseId != localWarehouseGuid)
            {
                logger.LogInformation("OrderCreatedConsumer: nearest warehouse {NearestWarehouseId} does not match local warehouse {LocalWarehouseId}. Skipping processing.", 
                    message.NearestWarehouseId, localWarehouseGuid);
                return;
            }
            logger.LogInformation("OrderCreatedConsumer: Nearest warehouse ID matches local warehouse {LocalWarehouseGuid}.", localWarehouseGuid);
        }
        else
        {
            logger.LogWarning("OrderCreatedConsumer: Invalid or missing WAREHOUSE_ID '{LocalWarehouseIdStr}'. Skipping processing of OrderCreatedEvent.", localWarehouseIdStr);
            return;
        }

        logger.LogInformation("OrderCreatedConsumer: Received matched OrderCreatedEvent for OrderId: {OrderId}", message.OrderId);

        var nearestWarehouseId = new WarehouseId(message.NearestWarehouseId);
        var orderId = new OrderId(message.OrderId);
        var noteCode = Code.Create($"PN-{message.OrderCode}");
        var deliveryInfo = new DeliveryInfo(
            message.ReceiverName,
            message.ReceiverPhone,
            message.DeliveryAddress,
            message.CompanyName,
            message.TaxCode
        );

        // Always create a Pending PickingNote
        var note = PickingNote.CreateForSalesOrder(
            nearestWarehouseId, 
            orderId, 
            noteCode, 
            $"Picking for Order {message.OrderCode}", 
            deliveryInfo,
            message.PickingNoteId != Guid.Empty ? new PickingNoteId(message.PickingNoteId) : null
        );

        logger.LogInformation("OrderCreatedConsumer: Adding {Count} line items to the picking note", message.LineItems.Count);
        foreach (var item in message.LineItems)
        {
            note.AddItem(
                new ProductId(item.ProductId), 
                Code.From(item.ProductCode), 
                item.ProductName, 
                item.Unit, 
                new Quantity(item.Quantity), 
                item.ManufactureName, 
                ""
            );
        }

        logger.LogInformation("OrderCreatedConsumer: Adding picking note to repository");
        await pickingNoteRepository.Add(note, context.CancellationToken);
        logger.LogInformation("OrderCreatedConsumer: Saving changes to DB via UnitOfWork");
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        
        logger.LogInformation("OrderCreatedConsumer: [END] Created Pending PickingNote {PickingNoteCode} from OrderCreatedEvent successfully.", note.Code.Value);
    }
}
