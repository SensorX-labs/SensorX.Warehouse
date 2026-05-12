using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;

namespace SensorX.Warehouse.Application.Queries.StockOuts;

public class GetPageListStockOutsHandler(
    IQueryBuilder<StockOut> _queryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListStockOutsQuery, Result<StockOutCursorPagedResult>>
{
    public async Task<Result<StockOutCursorPagedResult>> Handle(GetPageListStockOutsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _queryBuilder.QueryAsNoTracking
                .Where(x => x.WarehouseId == new Domain.StrongIDs.WarehouseId(request.WarehouseId));

            if (request.IsAdjustmentOnly)
            {
                query = query.Where(x => x.PickingNoteId == null);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(x => ((string)x.Code).Contains(term)
                    || (x.Description != null && x.Description.Contains(term)));
            }

            query = query.ApplyCursorPagination(
                request,
                x => x.CreatedAt,
                x => x.Id
            )
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

            var items = await _queryExecutor.ToListAsync(query
                .Select(x => new GetPageListStockOutsResponse(
                    x.Id.Value,
                    x.Code.Value,
                    x.Description,
                    x.PickingNoteId == null ? (Guid?)null : x.PickingNoteId.Value,
                    x.CreatedAt
                ))
                .Take(request.PageSize + 1),
                cancellationToken);

            var hasNext = items.Count > request.PageSize;
            if (hasNext) items.RemoveAt(request.PageSize);

            var result = new StockOutCursorPagedResult
            {
                Items = items,
                HasNext = hasNext,
                HasPrevious = request.IsPrevious,
                FirstCreatedAt = items.FirstOrDefault()?.CreatedAt,
                FirstId = items.FirstOrDefault()?.Id,
                LastCreatedAt = items.LastOrDefault()?.CreatedAt,
                LastId = items.LastOrDefault()?.Id
            };

            return Result<StockOutCursorPagedResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<StockOutCursorPagedResult>.Failure(ex.Message);
        }
    }
}