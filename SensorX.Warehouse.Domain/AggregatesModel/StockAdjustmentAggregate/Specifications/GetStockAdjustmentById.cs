using Ardalis.Specification;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate.Specifications;

public class GetStockAdjustmentById : SingleResultSpecification<StockAdjustment>
{
    public GetStockAdjustmentById(StockAdjustmentId id)
    {
        Query.Where(x => x.Id == id);
    }
}
