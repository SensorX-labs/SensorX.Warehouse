using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("transfer-order-finished")]
public record TransferOrderFinishedEvent
{
    public Guid TransferOrderId { get; init; }
    public Guid PickingNoteId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public DateTimeOffset FinishedAt { get; init; }
}
