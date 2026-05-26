using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.Services;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class OrderCreatedConsumer(
    ILogger<OrderCreatedConsumer> logger,
    IRepository<PickingNote> pickingNoteRepository,
    IRepository<InventoryItem> inventoryRepository,
    InventoryService inventoryService,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Received OrderCreatedEvent for OrderId: {OrderId}, Action: {ActionType}", message.OrderId, message.ActionType);

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

        if (message.ActionType == PickingAction.DirectPick)
        {
            var note = PickingNote.CreateForSalesOrder(nearestWarehouseId, orderId, noteCode, $"Picking for Order {message.OrderCode}", deliveryInfo);
            foreach (var item in message.LineItems)
            {
                note.AddItem(new ProductId(item.ProductId), Code.From(item.ProductCode), item.ProductName, item.Unit, new Quantity(item.Quantity), item.ManufactureName, "");
            }
            
            var productIds = message.LineItems.Select(x => new ProductId(x.ProductId)).ToList();
            var nearestSpec = new GetInventoryItemByProductIds(nearestWarehouseId, productIds);
            var nearestInventory = await inventoryRepository.ListAsync(nearestSpec, context.CancellationToken);

            inventoryService.StartPicking(nearestInventory, note); 
            
            await pickingNoteRepository.Add(note, context.CancellationToken);
            await inventoryRepository.UpdateRange(nearestInventory, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            
            logger.LogInformation("Created Pending PickingNote {PickingNoteCode} and allocated inventory", note.Code.Value);
        }
        else if (message.ActionType == PickingAction.WaitingTransfer)
        {
            // Master has created TransferOrder and sent PickingNoteId
            var note = PickingNote.CreateWaitingTransfer(
                nearestWarehouseId, 
                orderId, 
                noteCode, 
                $"Waiting transfer for Order {message.OrderCode}", 
                deliveryInfo,
                new PickingNoteId(message.PickingNoteId)
            );
                
            foreach (var item in message.LineItems)
            {
                note.AddItem(new ProductId(item.ProductId), Code.From(item.ProductCode), item.ProductName, item.Unit, new Quantity(item.Quantity), item.ManufactureName, "");
            }
            
            await pickingNoteRepository.Add(note, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            
            logger.LogInformation("Created WaitingTransfer PickingNote {PickingNoteCode}", note.Code.Value);
        }
        else if (message.ActionType == PickingAction.WaitingSupply)
        {
            // Master has created SupplyRequest and sent PickingNoteId
            var note = PickingNote.CreateWaitingSupply(
                nearestWarehouseId, 
                orderId, 
                noteCode, 
                $"Waiting supply for Order {message.OrderCode}", 
                deliveryInfo,
                new PickingNoteId(message.PickingNoteId)
            );
                
            foreach (var item in message.LineItems)
            {
                note.AddItem(new ProductId(item.ProductId), Code.From(item.ProductCode), item.ProductName, item.Unit, new Quantity(item.Quantity), item.ManufactureName, "");
            }
            
            await pickingNoteRepository.Add(note, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            
            logger.LogInformation("Created WaitingSupply PickingNote {PickingNoteCode}", note.Code.Value);
        }
    }
}
