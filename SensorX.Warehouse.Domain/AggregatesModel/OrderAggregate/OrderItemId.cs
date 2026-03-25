using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.OrderAggregate;

public record OrderItemId(Guid Id) : VoId(Id);