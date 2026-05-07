using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;

public record StockAdjustmentId(Guid Value) : EntityId<StockAdjustmentId>(Value)
{
    public static StockAdjustmentId New() => new(Guid.CreateVersion7());
}
