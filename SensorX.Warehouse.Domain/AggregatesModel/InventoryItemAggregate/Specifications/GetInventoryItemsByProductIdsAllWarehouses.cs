using System.Linq.Expressions;
using Ardalis.Specification;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;

public class GetInventoryItemsByProductIdsAllWarehouses : Specification<InventoryItem>
{
    public GetInventoryItemsByProductIdsAllWarehouses(List<ProductId> productIds)
    {
        Query.Where(x => productIds.Contains(x.ProductId));
    }
}
