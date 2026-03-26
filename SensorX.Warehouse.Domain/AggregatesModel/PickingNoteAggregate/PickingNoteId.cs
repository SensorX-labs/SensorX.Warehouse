using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public record PickingNoteId(Guid Value) : VoId(Value), IEntityId<PickingNoteId>
{
    public static PickingNoteId New() => new(Guid.NewGuid());
}