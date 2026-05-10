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

namespace SensorX.Warehouse.Application.Commands.RejectStockAdjustment;

public class RejectStockAdjustmentHandler(
    IRepository<StockAdjustment> _adjustmentRepository,
    IUnitOfWork _unitOfWork
) : IRequestHandler<RejectStockAdjustmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RejectStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        // 1. Load adjustment
        var spec = new GetStockAdjustmentById(new StockAdjustmentId(request.Id));
        var adjustment = await _adjustmentRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (adjustment is null)
            return Result<Guid>.Failure("Adjustment not found");

        // 2. Validate status
        if (adjustment.Status != AdjustmentStatus.Pending)
            return Result<Guid>.Failure($"Cannot reject adjustment in status {adjustment.Status}");

        // 3. Reject adjustment
        adjustment.Reject(request.Reason);

        // 4. Persist changes
        await _adjustmentRepository.Update(adjustment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(adjustment.Id.Value);
    }
}