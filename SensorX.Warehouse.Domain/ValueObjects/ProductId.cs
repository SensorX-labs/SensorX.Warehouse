
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record ProductId(Guid Value) : VoId(Value);