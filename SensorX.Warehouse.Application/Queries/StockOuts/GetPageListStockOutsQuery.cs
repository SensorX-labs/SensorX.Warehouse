using MediatR;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.StockOuts;

public class GetPageListStockOutsQuery : CursorPagedQuery, IRequest<Result<StockOutCursorPagedResult>>
{
    public Guid WarehouseId { get; set; }
    public bool IsAdjustmentOnly { get; set; }
    public string? SearchTerm { get; set; }
}

public record GetPageListStockOutsResponse(
    Guid Id,
    string Code,
    string? Description,
    Guid? PickingNoteId,
    DateTimeOffset CreatedAt
);

public class StockOutCursorPagedResult : CursorPagedResult<GetPageListStockOutsResponse> { }