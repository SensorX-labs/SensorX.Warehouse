using MediatR;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.StockAdjustments;

public class GetPageListStockAdjustmentsQuery : CursorPagedQuery, IRequest<Result<StockAdjustmentCursorPagedResult>>
{
    public string? SearchTerm { get; set; }
}

public record GetPageListStockAdjustmentsResponse(
    Guid Id,
    string Code,
    string? Reason,
    string Status,
    DateTimeOffset CreatedAt
);

public class StockAdjustmentCursorPagedResult : CursorPagedResult<GetPageListStockAdjustmentsResponse> { }