using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public record PickingLineItemId(Guid Value) : EntityId<PickingLineItemId>(Value);