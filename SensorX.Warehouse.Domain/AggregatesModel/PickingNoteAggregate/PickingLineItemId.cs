using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public record PickingLineItemId(Guid Value) : VoId(Value), IEntityId<PickingLineItemId>
{
    public static PickingLineItemId New() => new(Guid.NewGuid());
}