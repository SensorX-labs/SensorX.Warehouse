using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

public record InventoryItemId(Guid Value) : VoId(Value), IEntityId<InventoryItemId>
{
    public static InventoryItemId New() => new(Guid.NewGuid());
}