using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.OrderAggregate;

public record OrderId(Guid Id) : VoId(Id);