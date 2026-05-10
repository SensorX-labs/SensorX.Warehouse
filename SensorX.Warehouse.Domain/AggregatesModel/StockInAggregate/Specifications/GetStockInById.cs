using Ardalis.Specification;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate.Specifications;

public class GetStockInById : SingleResultSpecification<StockIn>
{
    public GetStockInById(StockInId id)
    {
        Query.Where(x => x.Id == id)
             .Include(x => x.LineItems);
    }
}
