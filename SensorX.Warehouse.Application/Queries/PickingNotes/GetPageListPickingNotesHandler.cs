using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

namespace SensorX.Warehouse.Application.Queries.PickingNotes;

public class GetPageListPickingNotesHandler(
    IQueryBuilder<PickingNote> _queryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListPickingNotesQuery, Result<PickingNoteCursorPagedResult>>
{
    public async Task<Result<PickingNoteCursorPagedResult>> Handle(GetPageListPickingNotesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _queryBuilder.QueryAsNoTracking
                .Where(x => x.WarehouseId == new Domain.StrongIDs.WarehouseId(request.WarehouseId));

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
                .Select(x => new GetPageListPickingNotesResponse(
                    x.Id.Value,
                    x.Code.Value,
                    x.Description,
                    x.Status.ToString(),
                    x.CreatedAt,
                    x.SourceDocument.Type == DocumentType.TransferOrder ? x.SourceDocument.Code : null
                ))
                .Take(request.PageSize + 1),
                cancellationToken);

            var hasNext = items.Count > request.PageSize;
            if (hasNext) items.RemoveAt(request.PageSize);

            var result = new PickingNoteCursorPagedResult
            {
                Items = items,
                HasNext = hasNext,
                HasPrevious = request.IsPrevious,
                FirstCreatedAt = items.FirstOrDefault()?.CreatedAt,
                FirstId = items.FirstOrDefault()?.Id,
                LastCreatedAt = items.LastOrDefault()?.CreatedAt,
                LastId = items.LastOrDefault()?.Id
            };

            return Result<PickingNoteCursorPagedResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PickingNoteCursorPagedResult>.Failure(ex.Message);
        }
    }
}
