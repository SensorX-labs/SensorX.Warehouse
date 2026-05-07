using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Commands.CreateStockAdjustment;

public class CreateStockAdjustmentHandler(
    IRepository<StockAdjustment> _adjustmentRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IUnitOfWork _unitOfWork
) : IRequestHandler<CreateStockAdjustmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create StockAdjustment aggregate
        var adjustment = new StockAdjustment(
            StockAdjustmentId.New(),
            Code.From(request.Code),
            request.Reason,
            request.Description
        );

        // 2. Add items
        foreach (var itemDto in request.Items)
        {
            adjustment.AddItem(
                new ProductId(itemDto.ProductId),
                Code.From(itemDto.ProductCode),
                itemDto.ProductName,
                itemDto.Unit,
                itemDto.AdjustedQuantity,
                itemDto.Note
            );
        }

        // 3. Persist
        await _adjustmentRepository.Add(adjustment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(adjustment.Id.Value);
    }
}