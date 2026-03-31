using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;

public record StockOutId(Guid Value) : EntityId<StockOutId>(Value);

