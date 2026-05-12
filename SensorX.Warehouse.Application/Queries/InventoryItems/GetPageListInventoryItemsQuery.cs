using MediatR;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.InventoryItems;

public class GetPageListInventoryItemsQuery : CursorPagedQuery, IRequest<Result<InventoryItemCursorPagedResult>>
{
    public Guid WarehouseId { get; set; }
    public string? SearchTerm { get; set; }
}

public record GetPageListInventoryItemsResponse(
    Guid Id,
    Guid ProductId,
    decimal PhysicalQuantity,
    decimal AllocatedQuantity,
    string? WarehouseName,
    string? BrandZone,
    string? RackCode,
    DateTimeOffset CreatedAt
);

public class InventoryItemCursorPagedResult : CursorPagedResult<GetPageListInventoryItemsResponse> { }
