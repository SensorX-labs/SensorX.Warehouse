using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public record PickingNoteId(Guid Value) : EntityId<PickingNoteId>(Value);

