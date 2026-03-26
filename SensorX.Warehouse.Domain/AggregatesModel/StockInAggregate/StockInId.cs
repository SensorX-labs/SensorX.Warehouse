using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

public record StockInId(Guid Value) : EntityId<StockInId>(Value);