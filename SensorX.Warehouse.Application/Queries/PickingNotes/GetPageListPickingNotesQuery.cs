using MediatR;
using SensorX.Warehouse.Application.Common.Pagination;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.PickingNotes;

public class GetPageListPickingNotesQuery : CursorPagedQuery, IRequest<Result<PickingNoteCursorPagedResult>>
{
    public Guid WarehouseId { get; set; }
    public string? SearchTerm { get; set; }
}

public record GetPageListPickingNotesResponse(
    Guid Id,
    string Code,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt
);

public class PickingNoteCursorPagedResult : CursorPagedResult<GetPageListPickingNotesResponse> { }