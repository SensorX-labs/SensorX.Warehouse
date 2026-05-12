using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.StrongIDs;

public record OrderId(Guid Value) : EntityId<OrderId>(Value);
