using Ardalis.Specification;

namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;

public class GetInventoryItemByProductIds : Specification<InventoryItem>
{
    public GetInventoryItemByProductIds(List<Guid> productIds)
    {
        Query.Where(x => productIds.Contains(x.ProductId));
    }
}