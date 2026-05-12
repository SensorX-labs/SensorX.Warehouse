using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

public record StockInItemId(Guid Value) : EntityId<StockInItemId>(Value);

