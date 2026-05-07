using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Queries.GetStockAdjustment;

public class GetStockAdjustmentHandler(
    IRepository<StockAdjustment> _adjustmentRepository
) : IRequestHandler<GetStockAdjustmentQuery, Result<StockAdjustmentDto>>
{
    public async Task<Result<StockAdjustmentDto>> Handle(GetStockAdjustmentQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetStockAdjustmentById(new StockAdjustmentId(request.Id));
        var adjustment = await _adjustmentRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (adjustment == null)
            return Result<StockAdjustmentDto>.Failure("Adjustment not found");

        var dto = new StockAdjustmentDto(
            adjustment.Id.Value,
            adjustment.Code.Value,
            adjustment.Reason,
            adjustment.Description,
            adjustment.Status.ToString(),
            adjustment.Items.Select(item => new StockAdjustmentQueryItemDto(
                item.ProductId.Value,
                item.ProductCode.Value,
                item.ProductName,
                item.Unit,
                item.AdjustedQuantity,
                item.Note
            )).ToList()
        );

        return Result<StockAdjustmentDto>.Success(dto);
    }
}