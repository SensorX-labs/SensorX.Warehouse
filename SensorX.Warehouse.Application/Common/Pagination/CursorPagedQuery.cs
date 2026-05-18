namespace SensorX.Warehouse.Application.Common.Pagination;

/// <summary>
/// Base query for cursor-based pagination.
/// </summary>
public class CursorPagedQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int PageSize { get; set; } = DefaultPageSize;
    public bool IsPrevious { get; set; }
    public DateTimeOffset? FirstCreatedAt { get; set; }
    public Guid? FirstId { get; set; }
    public DateTimeOffset? LastCreatedAt { get; set; }
    public Guid? LastId { get; set; }
}