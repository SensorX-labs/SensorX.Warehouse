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
            var baseQuery = _queryBuilder.QueryAsNoTracking
                .Where(x => x.WarehouseItemLocation != null && x.WarehouseItemLocation.WarehouseId == new Domain.StrongIDs.WarehouseId(request.WarehouseId));

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                
                var productQuery = _productQueryBuilder.QueryAsNoTracking
                    .Where(p => p.Name.Contains(term) || p.Code.Contains(term))
                    .Select(p => p.Id);

                var matchingProductIds = await _queryExecutor.ToListAsync(productQuery, cancellationToken);

                var productIdsList = matchingProductIds.Select(id => new Domain.StrongIDs.ProductId(id)).ToList();

                baseQuery = baseQuery.Where(x => 
                    productIdsList.Contains(x.ProductId)
                    || (x.WarehouseItemLocation != null && x.WarehouseItemLocation.WarehouseName.Contains(term))
                    || (x.WarehouseItemLocation != null && x.WarehouseItemLocation.RackCode != null && x.WarehouseItemLocation.RackCode.Contains(term)));
            }

            var pagedQuery = baseQuery.ApplyCursorPagination(
                request,
                x => x.CreatedAt,
                x => x.Id
            )
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

            var itemsList = await _queryExecutor.ToListAsync(pagedQuery.Take(request.PageSize + 1), cancellationToken);

            var distinctProductIds = itemsList.Select(x => x.ProductId.Value).Distinct().ToList();
            var productsQuery = _productQueryBuilder.QueryAsNoTracking
                .Where(p => distinctProductIds.Contains(p.Id));

            var productsList = await _queryExecutor.ToListAsync(productsQuery, cancellationToken);
            var products = productsList.ToDictionary(p => p.Id.Value);

            var items = itemsList.Select(x =>
            {
                products.TryGetValue(x.ProductId.Value, out var product);
                return new GetPageListInventoryItemsResponse(
                    x.Id.Value,
                    x.ProductId.Value,
                    product?.Name,
                    product?.Code,
                    x.PhysicalQuantity.Value,
                    x.AllocatedQuantity.Value,
                    x.WarehouseItemLocation?.WarehouseName,
                    x.WarehouseItemLocation?.BrandZone,
                    x.WarehouseItemLocation?.RackCode,
                    x.CreatedAt
                );
            }).ToList();

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
