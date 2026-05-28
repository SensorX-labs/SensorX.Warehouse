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

public class UpdateProductConsumer(
    IRepository<ProductReadModel> _productRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IUnitOfWork _unitOfWork,
    IPublishEndpoint _publishEndpoint,
    IConfiguration _configuration,
    ILogger<UpdateProductConsumer> _logger
) : IConsumer<UpdateProductEvent>
{
    public async Task Consume(ConsumeContext<UpdateProductEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming UpdateProductEvent for ProductId: {ProductId}", message.Id);

        var productId = new ProductId(message.Id);
        var product = await _productRepository.GetByIdAsync(productId, context.CancellationToken);

        if (product != null)
        {
            product.Update(
                product.Code,
                message.Name,
                message.Unit,
                message.Manufacture,
                product.Status
            );
            await _productRepository.Update(product, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Successfully updated ProductReadModel for ProductId: {ProductId}", message.Id);

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
                    _logger.LogInformation("Found {Count} inventory items for updated product {ProductId}. Publishing snapshot update.", 
                        inventoryItems.Count, message.Id);
                    
                    var snapshotItems = inventoryItems.Select(item => 
                        new InventoryItemSnapshot(
                            item.ProductId.Value,
                            product.Code,
                            product.Name,
                            product.Unit,
                            item.PhysicalQuantity.Value,
                            item.AllocatedQuantity.Value,
                            item.WarehouseItemLocation?.WarehouseName,
                            item.WarehouseItemLocation?.BrandZone,
                            item.WarehouseItemLocation?.RackCode
                        )
                    ).ToList();

                    await _publishEndpoint.Publish(new InventorySnapshotEvent(warehouseId, DateTimeOffset.UtcNow, snapshotItems), context.CancellationToken);
                    _logger.LogInformation("Published inventory snapshot with {Count} items for updated product {ProductId}", 
                        snapshotItems.Count, message.Id);
                }
            }
            else
            {
                _logger.LogWarning("Invalid WAREHOUSE_ID {WarehouseId}. Skipping inventory snapshot update.", warehouseId);
            }
        }
        else
        {
            _logger.LogWarning("ProductReadModel not found for ProductId: {ProductId} during UpdateProductEvent", message.Id);
        }
    }
}
