using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

namespace SensorX.Warehouse.Application.Queries.StockIns;

public class GetPageListStockInsHandler(
    IQueryBuilder<StockIn> _queryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListStockInsQuery, Result<StockInCursorPagedResult>>
{
    public async Task<Result<StockInCursorPagedResult>> Handle(GetPageListStockInsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _queryBuilder.QueryAsNoTracking
                .Where(x => x.WarehouseId == new Domain.StrongIDs.WarehouseId(request.WarehouseId));

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(x => ((string)x.Code).Contains(term)
                    || (x.Description != null && x.Description.Contains(term))
                    || x.CreatedBy.Contains(term));
            }

            query = query.ApplyCursorPagination(
                request,
                x => x.CreatedAt,
                x => x.Id
            )
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

            var items = await _queryExecutor.ToListAsync(query
                .Select(x => new GetPageListStockInsResponse(
                    x.Id.Value,
                    x.Code.Value,
                    x.Description,
                    x.CreatedBy,
                    x.CreatedAt
                ))
                .Take(request.PageSize + 1),
                cancellationToken);

            var hasNext = items.Count > request.PageSize;
            if (hasNext) items.RemoveAt(request.PageSize);

            var result = new StockInCursorPagedResult
            {
                Items = items,
                HasNext = hasNext,
                HasPrevious = request.IsPrevious,
                FirstCreatedAt = items.FirstOrDefault()?.CreatedAt,
                FirstId = items.FirstOrDefault()?.Id,
                LastCreatedAt = items.LastOrDefault()?.CreatedAt,
                LastId = items.LastOrDefault()?.Id
            };

            return Result<StockInCursorPagedResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<StockInCursorPagedResult>.Failure(ex.Message);
        }
    }
}
