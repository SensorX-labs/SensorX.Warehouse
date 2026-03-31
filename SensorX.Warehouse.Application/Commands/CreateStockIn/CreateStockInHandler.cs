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
        var spec = new GetInventoryItemByProductIds([.. request.Items.Select(x => x.ProductId)]);
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

        var stockIn = _inventoryService.CreateStockIn(
             inventoryItems,
             lineItems,
             transferOrderCode,
             request.Description,
             DateTimeOffset.Now,
             _currentUser.Username!,
             request.DevliveredBy,
             request.WarehouseKeeper
         );

        await _stockInRepository.Add(stockIn, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(stockIn.Id.Value);
    }
}