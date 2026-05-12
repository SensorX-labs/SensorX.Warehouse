using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

namespace SensorX.Warehouse.Application.Queries.InventoryItems;

public class GetPageListInventoryItemsHandler(
    IQueryBuilder<InventoryItem> _queryBuilder,
    IQueryBuilder<SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate.ProductReadModel> _productQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListInventoryItemsQuery, Result<InventoryItemCursorPagedResult>>
{
    public async Task<Result<InventoryItemCursorPagedResult>> Handle(GetPageListInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var baseQuery = from i in _queryBuilder.QueryAsNoTracking.Where(x => x.WarehouseItemLocation.WarehouseId == new Domain.StrongIDs.WarehouseId(request.WarehouseId))
                            join p in _productQueryBuilder.QueryAsNoTracking on i.ProductId equals p.Id into pj
                            from p in pj.DefaultIfEmpty()
                            select new { Item = i, Product = p };

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                baseQuery = baseQuery.Where(x => 
                    (x.Product != null && x.Product.Name.Contains(term))
                    || (x.Product != null && x.Product.Code.Contains(term))
                    || (x.Item.WarehouseItemLocation != null && x.Item.WarehouseItemLocation.WarehouseName.Contains(term))
                    || (x.Item.WarehouseItemLocation != null && x.Item.WarehouseItemLocation.RackCode != null && x.Item.WarehouseItemLocation.RackCode.Contains(term)));
            }

            var pagedQuery = baseQuery.ApplyCursorPagination(
                request,
                x => x.Item.CreatedAt,
                x => x.Item.Id.Value
            )
            .OrderByDescending(x => x.Item.CreatedAt)
            .ThenByDescending(x => x.Item.Id.Value);

            var items = await _queryExecutor.ToListAsync(pagedQuery
                .Select(x => new GetPageListInventoryItemsResponse(
                    x.Item.Id.Value,
                    x.Item.ProductId.Value,
                    x.Product != null ? x.Product.Name : null,
                    x.Product != null ? x.Product.Code : null,
                    x.Item.PhysicalQuantity.Value,
                    x.Item.AllocatedQuantity.Value,
                    x.Item.WarehouseItemLocation != null ? x.Item.WarehouseItemLocation.WarehouseName : null,
                    x.Item.WarehouseItemLocation != null ? x.Item.WarehouseItemLocation.BrandZone : null,
                    x.Item.WarehouseItemLocation != null ? x.Item.WarehouseItemLocation.RackCode : null,
                    x.Item.CreatedAt
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
