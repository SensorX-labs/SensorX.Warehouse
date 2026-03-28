using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record CategoryId(Guid Value) : EntityId<CategoryId>(Value);