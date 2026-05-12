using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.StrongIDs;

public record TransferOrderId(Guid Value) : EntityId<TransferOrderId>(Value);
