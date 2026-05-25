using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Infrastructure.Persistences;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;

namespace SensorX.Warehouse.Infrastructure.Services;

public class WarehouseStartupPublisher : IHostedService
{
    private readonly ILogger<WarehouseStartupPublisher> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public WarehouseStartupPublisher(
        ILogger<WarehouseStartupPublisher> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var warehouseId = _configuration["WAREHOUSE_ID"] ?? _configuration["Warehouse:Id"];
        if (string.IsNullOrWhiteSpace(warehouseId))
        {
            _logger.LogWarning("WAREHOUSE_ID (or Warehouse:Id) not configured. Skipping startup publish.");
            return;
        }

        var warehouseName = _configuration["WAREHOUSE_NAME"] ?? _configuration["Warehouse:Name"] ?? $"Warehouse {warehouseId[..Math.Min(warehouseId.Length, 8)]}";
        var warehouseAddress = _configuration["WAREHOUSE_ADDRESS"] ?? _configuration["Warehouse:Address"] ?? "Unknown Address";
        _logger.LogInformation("Publishing WarehouseConnected for {WarehouseId} ({WarehouseName}) at {WarehouseAddress}", warehouseId, warehouseName, warehouseAddress);

        using var scope = _serviceProvider.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IBus>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await bus.Publish(new WarehouseConnectedEvent(warehouseId, warehouseName, warehouseAddress, "connected", DateTimeOffset.UtcNow), cancellationToken);

        if (!Guid.TryParse(warehouseId, out var warehouseGuid))
        {
            _logger.LogWarning("WAREHOUSE_ID value '{WarehouseId}' is not a valid GUID. Skip inventory snapshot publish.", warehouseId);
            return;
        }

        var inventoryRows = await db.Set<InventoryItem>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productIds = inventoryRows.Select(x => x.ProductId).Distinct().ToList();
        
        // Wait for all products to be synced before publishing snapshot
        // This ensures ProductReadModel has complete data for all products
        const int maxWaitRetries = 60;  // 60 * 1 second = 1 minute max wait
        int retryCount = 0;
        
        while (retryCount < maxWaitRetries)
        {
            var syncedProducts = await db.Set<ProductReadModel>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            // Check if all products exist AND have complete data (not null)
            var completeProducts = syncedProducts
                .Where(p => !string.IsNullOrWhiteSpace(p.Code) && 
                           !string.IsNullOrWhiteSpace(p.Name) && 
                           !string.IsNullOrWhiteSpace(p.Unit))
                .ToList();

            if (syncedProducts.Count == productIds.Count && completeProducts.Count == productIds.Count)
            {
                _logger.LogInformation("All {Count} products synced with complete data successfully", productIds.Count);
                break;
            }

            var missingCount = productIds.Count - syncedProducts.Count;
            var incompleteCount = syncedProducts.Count - completeProducts.Count;
            
            _logger.LogInformation("Waiting for products to sync... (Synced: {SyncedCount}/{Total}, Complete: {CompleteCount}/{Total}, Missing: {Missing}, Incomplete: {Incomplete}), retry {Retry}/{MaxRetries}", 
                syncedProducts.Count, productIds.Count, completeProducts.Count, productIds.Count, missingCount, incompleteCount, retryCount + 1, maxWaitRetries);

            await Task.Delay(1000, cancellationToken);  // Wait 1 second before retry
            retryCount++;
        }

        if (retryCount >= maxWaitRetries)
        {
            _logger.LogWarning("Timeout waiting for all products to sync. Proceeding with available products. This may result in incomplete inventory snapshots.");
        }

        var products = await db.Set<ProductReadModel>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var items = inventoryRows
            .Where(inventory => inventory.WarehouseItemLocation != null && inventory.WarehouseItemLocation.WarehouseId.Value == warehouseGuid)
            .Select(inventory =>
            {
                products.TryGetValue(inventory.ProductId, out var product);
                
                // Log warning if product details missing, but still include the item
                if (product == null)
                {
                    _logger.LogWarning("Product not found for InventoryItem {InventoryItemId} (ProductId: {ProductId}). Publishing with null product details.", 
                        inventory.Id.Value, inventory.ProductId.Value);
                }
                
                return new InventoryItemSnapshot(
                    inventory.ProductId.Value,
                    product?.Code,
                    product?.Name,
                    product?.Unit,
                    inventory.PhysicalQuantity.Value,
                    inventory.AllocatedQuantity.Value,
                    inventory.WarehouseItemLocation?.WarehouseName,
                    inventory.WarehouseItemLocation?.BrandZone,
                    inventory.WarehouseItemLocation?.RackCode
                );
            })
            .ToList();

        _logger.LogInformation("Publishing InventorySnapshot with {Count} items for {WarehouseId}", items.Count, warehouseId);

        await bus.Publish(new InventorySnapshotEvent(warehouseId, DateTimeOffset.UtcNow, items), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
