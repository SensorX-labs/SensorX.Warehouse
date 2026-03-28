using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record TransferOrderId(Guid Value) : EntityId<TransferOrderId>(Value);