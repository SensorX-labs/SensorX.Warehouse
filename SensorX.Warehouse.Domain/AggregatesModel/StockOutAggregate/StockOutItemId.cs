using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;

public record StockOutItemId(Guid Value) : EntityId<StockOutItemId>(Value);

