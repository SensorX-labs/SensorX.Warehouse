using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.StrongIDs;

public record CategoryId(Guid Value) : EntityId<CategoryId>(Value);
