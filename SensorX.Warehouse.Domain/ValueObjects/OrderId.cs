using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record OrderId(Guid Value) : EntityId<OrderId>(Value);