
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.ValueObjects;

public record ProductId(Guid Id) : VoId(Id);