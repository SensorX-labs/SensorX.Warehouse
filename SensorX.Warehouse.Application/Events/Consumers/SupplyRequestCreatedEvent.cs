using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("supply-request-created")]
public record SupplyRequestCreatedEvent
{
    public Guid SupplyRequestId { get; init; }
    public Guid PickingNoteId { get; init; }
    public Guid WarehouseId { get; init; }
}
