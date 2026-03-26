
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record WarehouseId(Guid Value) : VoId(Value), IEntityId<WarehouseId>
{
    public static WarehouseId New() => new(Guid.NewGuid());
}