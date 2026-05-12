using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("product-deleted")]
public record ProductDeletedEvent
{
    public Guid ProductId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
