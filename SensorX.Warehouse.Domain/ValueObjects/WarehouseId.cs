
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record WarehouseId(Guid Value) : VoId(Value), IEntityId<WarehouseId>
{
    public static WarehouseId New() => new(Guid.NewGuid());
    public static readonly WarehouseId Default = new(Guid.Parse("018e7b5a-1a5c-7b9e-8d4f-c3b2a1a0b9c8"));
}