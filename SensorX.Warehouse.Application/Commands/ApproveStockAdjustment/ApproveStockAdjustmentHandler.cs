using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Commands.ApproveStockAdjustment;

public class ApproveStockAdjustmentHandler(
    IRepository<StockAdjustment> _adjustmentRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService
) : IRequestHandler<ApproveStockAdjustmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ApproveStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Load adjustment
        var spec = new GetStockAdjustmentById(new StockAdjustmentId(request.Id));
        var adjustment = await _adjustmentRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (adjustment is null)
            return Result<Guid>.Failure("Adjustment not found");

        // 2. Validate status
        if (adjustment.Status != AdjustmentStatus.Pending)
            return Result<Guid>.Failure($"Cannot approve adjustment in status {adjustment.Status}");

        // 3. Load inventory items referenced
        var productIds = adjustment.Items.Select(x => x.ProductId).Distinct().ToList();
        var inventorySpec = new GetInventoryItemByProductIds([.. productIds]);
        var inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

        // 4. Approve adjustment
        adjustment.Approve();

        // 5. Apply adjustment to inventory
        _inventoryService.ApplyAdjustment(inventoryItems, adjustment);

        // 6. Persist changes
        await _adjustmentRepository.Update(adjustment, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(adjustment.Id.Value);
    }
}