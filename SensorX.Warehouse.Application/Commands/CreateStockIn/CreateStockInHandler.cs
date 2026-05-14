using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.Services.DTOs;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Commands.CreateStockIn;

public class CreateStockInHandler(
    IRepository<InventoryItem> _inventoryItemRepository,
    IRepository<StockIn> _stockInRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService,
    ICurrentUser _currentUser
) : IRequestHandler<CreateStockInCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStockInCommand request, CancellationToken cancellationToken)
    {
        var warehouseId = new WarehouseId(request.WarehouseId);
        var spec = new GetInventoryItemByProductIds(warehouseId, [.. request.Items.Select(x => new ProductId(x.ProductId))]);
        var lineItems = request.Items.Select(x => new StockInLineRequest
        {
            ProductId = new ProductId(x.ProductId),
            ProductCode = Code.From(x.ProductCode),
            ProductName = x.ProductName,
            Unit = x.Unit,
            Quantity = new Quantity(x.Quantity)
        }).ToList();

        var transferOrderCode = request.TransferOrderCode != null ? Code.From(request.TransferOrderCode) : null;
        var inventoryItems = await _inventoryItemRepository.ListAsync(spec, cancellationToken);

        var allInventoryItems = new List<InventoryItem>(inventoryItems);
        var existingProductIds = inventoryItems.Select(x => x.ProductId).ToHashSet();
        foreach (var reqItem in lineItems)
        {
            if (!existingProductIds.Contains(reqItem.ProductId))
            {
                var newItem = new InventoryItem(
                    InventoryItemId.New(),
                    reqItem.ProductId,
                    new WarehouseItemLocation(warehouseId, "Kho chính", "Tầng 1", "Khu A", "Kệ 01"),
                    new Quantity(0),
                    new Quantity(0)
                );
                await _inventoryItemRepository.Add(newItem, cancellationToken);
                allInventoryItems.Add(newItem);
                existingProductIds.Add(reqItem.ProductId);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(stockIn.Id.Value);
    }
}