using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

namespace SensorX.Warehouse.Application.Queries.InventoryItems;

public class GetPageListInventoryItemsHandler(
    IQueryBuilder<InventoryItem> _queryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListInventoryItemsQuery, Result<InventoryItemCursorPagedResult>>
{
    public async Task<Result<InventoryItemCursorPagedResult>> Handle(GetPageListInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _queryBuilder.QueryAsNoTracking;

            // Note: Since InventoryItem only has ProductId, searchTerm here can only match Warehouse or Location
            // If we want to search by ProductName, we would need a join or fetch products first.
            // For now, we search by location info.
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(x => 
                    (x.WarehouseItemLocation != null && x.WarehouseItemLocation.WarehouseName.Contains(term))
                    || (x.WarehouseItemLocation != null && x.WarehouseItemLocation.RackCode != null && x.WarehouseItemLocation.RackCode.Contains(term)));
            }

            query = query.ApplyCursorPagination(
                request,
                x => x.CreatedAt,
                x => x.Id
            )
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

            var items = await _queryExecutor.ToListAsync(query
                .Select(x => new GetPageListInventoryItemsResponse(
                    x.Id.Value,
                    x.ProductId.Value,
                    x.PhysicalQuantity.Value,
                    x.AllocatedQuantity.Value,
                    x.WarehouseItemLocation != null ? x.WarehouseItemLocation.WarehouseName : null,
                    x.WarehouseItemLocation != null ? x.WarehouseItemLocation.BrandZone : null,
                    x.WarehouseItemLocation != null ? x.WarehouseItemLocation.RackCode : null,
                    x.CreatedAt
                ))
                .Take(request.PageSize + 1),
                cancellationToken);

            var hasNext = items.Count > request.PageSize;
            if (hasNext) items.RemoveAt(request.PageSize);

            var result = new InventoryItemCursorPagedResult
            {
                Items = items,
                HasNext = hasNext,
                HasPrevious = request.IsPrevious,
                FirstCreatedAt = items.FirstOrDefault()?.CreatedAt,
                FirstId = items.FirstOrDefault()?.Id,
                LastCreatedAt = items.LastOrDefault()?.CreatedAt,
                LastId = items.LastOrDefault()?.Id
            };

            return Result<InventoryItemCursorPagedResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<InventoryItemCursorPagedResult>.Failure(ex.Message);
        }
    }
}
