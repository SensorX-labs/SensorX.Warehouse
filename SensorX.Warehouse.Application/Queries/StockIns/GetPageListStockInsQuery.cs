using MediatR;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.StockIns;

public class GetPageListStockInsQuery : CursorPagedQuery, IRequest<Result<StockInCursorPagedResult>>
{
    public Guid WarehouseId { get; set; }
    public string? SearchTerm { get; set; }
}

public record GetPageListStockInsResponse(
    Guid Id,
    string Code,
    string? Description,
    string CreatedBy,
    DateTimeOffset CreatedAt
);

public class StockInCursorPagedResult : CursorPagedResult<GetPageListStockInsResponse> { }