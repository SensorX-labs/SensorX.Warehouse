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
        var products = await db.Set<ProductReadModel>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var items = inventoryRows
            .Where(inventory => inventory.WarehouseItemLocation != null && inventory.WarehouseItemLocation.WarehouseId.Value == warehouseGuid)
            .Select(inventory =>
            {
                products.TryGetValue(inventory.ProductId, out var product);
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
