using MediatR;
using MassTransit;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Application.Events;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.Services.DTOs;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace SensorX.Warehouse.Application.Commands.CreateStockIn;

public class CreateStockInHandler(
    IRepository<InventoryItem> _inventoryItemRepository,
    IRepository<StockIn> _stockInRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService,
    IPublishEndpoint _publishEndpoint,
    ICurrentUser _currentUser,
    IConfiguration _configuration
) : IRequestHandler<CreateStockInCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStockInCommand request, CancellationToken cancellationToken)
    {
        var warehouseId = new WarehouseId(request.WarehouseId);
        var warehouseGuid = request.WarehouseId;
        var spec = new GetInventoryItemByProductIds(warehouseId, [.. request.Items.Select(x => new ProductId(x.ProductId))]);
        var lineItems = request.Items.Select(x => new StockInLineRequest
        {
            ProductId = new ProductId(x.ProductId),
            ProductCode = Code.From(x.ProductCode),
            ProductName = x.ProductName,
            Unit = x.Unit,
            Quantity = new Quantity(x.Quantity)
        }).ToList();

        var rawCode = request.TransferOrderCode ?? request.LinkedTransferOrderCode;
        var transferOrderCode = rawCode != null ? Code.From(rawCode) : null;
        var inventoryItems = await _inventoryItemRepository.ListAsync(spec, cancellationToken);

        var allInventoryItems = new List<InventoryItem>(inventoryItems);
        var existingProductIds = inventoryItems.Select(x => x.ProductId).ToHashSet();
        foreach (var reqItem in request.Items)
        {
            var productId = new ProductId(reqItem.ProductId);
            if (!existingProductIds.Contains(productId))
            {
                var floor = !string.IsNullOrWhiteSpace(reqItem.Floor) ? reqItem.Floor : "Tầng 1";
                var brandZone = !string.IsNullOrWhiteSpace(reqItem.BrandZone) ? reqItem.BrandZone : "Khu A";
                var rackCode = !string.IsNullOrWhiteSpace(reqItem.RackCode) ? reqItem.RackCode : "Kệ 01";

                var newItem = new InventoryItem(
                    InventoryItemId.New(),
                    productId,
                    new WarehouseItemLocation(warehouseId, _configuration["WAREHOUSE_NAME"] ?? _configuration["Warehouse:Name"] ?? "Không xác định", floor, brandZone, rackCode),
                    new Quantity(0),
                    new Quantity(0)
                );
                await _inventoryItemRepository.Add(newItem, cancellationToken);
                allInventoryItems.Add(newItem);
                existingProductIds.Add(productId);
            }
            else
            {
                var existingItem = allInventoryItems.First(x => x.ProductId == productId);
                var floor = !string.IsNullOrWhiteSpace(reqItem.Floor) ? reqItem.Floor : existingItem.WarehouseItemLocation?.Floor ?? "Tầng 1";
                var brandZone = !string.IsNullOrWhiteSpace(reqItem.BrandZone) ? reqItem.BrandZone : existingItem.WarehouseItemLocation?.BrandZone ?? "Khu A";
                var rackCode = !string.IsNullOrWhiteSpace(reqItem.RackCode) ? reqItem.RackCode : existingItem.WarehouseItemLocation?.RackCode ?? "Kệ 01";

                existingItem.UpdateLocation(new WarehouseItemLocation(
                    warehouseId,
                    _configuration["WAREHOUSE_NAME"] ?? _configuration["Warehouse:Name"] ?? "Không xác định",
                    floor,
                    brandZone,
                    rackCode
                ));
            }
        }

        var stockIn = _inventoryService.CreateStockIn(
            warehouseId,
            allInventoryItems,
            lineItems,
            transferOrderCode,
            request.Description,
            DateTimeOffset.UtcNow,
            _currentUser.Username ?? "unknown",
            request.DeliveredBy,
            request.WarehouseKeeper
        );

        await _stockInRepository.Add(stockIn, cancellationToken);
        if (inventoryItems.Count > 0)
        {
            await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        }

        var snapshotItems = allInventoryItems
            .Where(inventory => inventory.WarehouseItemLocation != null && inventory.WarehouseItemLocation.WarehouseId.Value == warehouseGuid)
            .Select(inventory =>
            {
                var lineItem = lineItems.FirstOrDefault(x => x.ProductId == inventory.ProductId);
                var product = lineItem != null 
                    ? new InventoryItemSnapshot(
                        inventory.ProductId.Value,
                        lineItem.ProductCode.Value,
                        lineItem.ProductName,
                        lineItem.Unit,
                        inventory.PhysicalQuantity.Value,
                        inventory.AllocatedQuantity.Value,
                        inventory.WarehouseItemLocation?.WarehouseName,
                        inventory.WarehouseItemLocation?.BrandZone,
                        inventory.WarehouseItemLocation?.RackCode
                    )
                    : new InventoryItemSnapshot(
                        inventory.ProductId.Value,
                        null,  // Product details might be null for existing items
                        null,
                        null,
                        inventory.PhysicalQuantity.Value,
                        inventory.AllocatedQuantity.Value,
                        inventory.WarehouseItemLocation?.WarehouseName,
                        inventory.WarehouseItemLocation?.BrandZone,
                        inventory.WarehouseItemLocation?.RackCode
                    );
                return product;
            })
            .ToList();

        await _publishEndpoint.Publish(new InventorySnapshotEvent(request.WarehouseId.ToString(), DateTimeOffset.UtcNow, snapshotItems), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(stockIn.Id.Value);
    }
}