using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("product-sync")]
public record ProductSyncEvent
{
    public Guid ProductId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string Manufacture { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
}
