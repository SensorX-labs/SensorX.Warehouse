using System.Collections.Generic;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Events.Consumers;

public class CreateProductConsumer(
    IRepository<ProductReadModel> _productRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IUnitOfWork _unitOfWork,
    IPublishEndpoint _publishEndpoint,
    IConfiguration _configuration,
    ILogger<CreateProductConsumer> _logger
) : IConsumer<CreateProductEvent>
{
    public async Task Consume(ConsumeContext<CreateProductEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming CreateProductEvent for ProductId: {ProductId}, Code: {Code}", message.Id, message.Code);

        var productId = new ProductId(message.Id);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product == null)
        {
            _logger.LogInformation("Creating new ProductReadModel for ProductId: {ProductId}", message.Id);
            product = new ProductReadModel(
                productId,
                message.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                message.Status.ToString()
            );
            await _productRepository.Add(product, context.CancellationToken);
        }
        else
        {
            _logger.LogInformation("Updating existing ProductReadModel for ProductId: {ProductId}", message.Id);
            product.Update(
                message.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                message.Status.ToString()
            );
            await _productRepository.Update(product, context.CancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Successfully processed CreateProductEvent for product: {Code}", message.Code);

        // Publish updated inventory snapshot for this product to notify Master service
        // This ensures Master always has the latest product information
        var warehouseId = _configuration["WAREHOUSE_ID"] ?? _configuration["Warehouse:Id"];
        if (Guid.TryParse(warehouseId, out var warehouseGuid))
        {
            // Get all inventory items for this product in this warehouse
            var warehouseStrongId = new WarehouseId(warehouseGuid);
            var spec = new GetInventoryItemByProductIds(warehouseStrongId, new List<ProductId> { productId });
            var inventoryItems = await _inventoryItemRepository.ListAsync(spec, context.CancellationToken);
            
            if (inventoryItems.Any())
            {
                _logger.LogInformation("Found {Count} inventory items for new product {ProductId}. Publishing snapshot update.", 
                    inventoryItems.Count, message.Id);
                
                var snapshotItems = inventoryItems.Select(item => 
                    new InventoryItemSnapshot(
                        item.ProductId.Value,
                        message.Code,
                        message.Name,
                        message.Unit,
                        item.PhysicalQuantity.Value,
                        item.AllocatedQuantity.Value,
                        item.WarehouseItemLocation?.WarehouseName,
                        item.WarehouseItemLocation?.BrandZone,
                        item.WarehouseItemLocation?.RackCode
                    )
                ).ToList();

                await _publishEndpoint.Publish(new InventorySnapshotEvent(warehouseId, DateTimeOffset.UtcNow, snapshotItems), context.CancellationToken);
                _logger.LogInformation("Published inventory snapshot with {Count} items for new product {Code}", 
                    snapshotItems.Count, message.Code);
            }
            else
            {
                _logger.LogInformation("No inventory items found for new product {ProductId} in warehouse {WarehouseId}. Snapshot not published.", 
                    message.Id, warehouseId);
            }
        }
        else
        {
            _logger.LogWarning("Invalid WAREHOUSE_ID {WarehouseId}. Skipping inventory snapshot update.", warehouseId);
        }
    }
}
