using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("transfer-order-created")]
public record TransferOrderCreatedEvent
{
    public Guid TransferOrderId { get; init; }
    public string TransferOrderCode { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}