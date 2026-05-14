
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.StrongIDs;

public record WarehouseId(Guid Value) : EntityId<WarehouseId>(Value);
