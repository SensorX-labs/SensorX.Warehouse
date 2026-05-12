using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

public record InventoryItemId(Guid Value) : EntityId<InventoryItemId>(Value);

