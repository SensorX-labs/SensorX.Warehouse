using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("supply-request-fulfilled")]
public record SupplyRequestFulfilledEvent
{
    public Guid SupplyRequestId { get; init; }
    public Guid PickingNoteId { get; init; }
    public Guid WarehouseId { get; init; }
}
