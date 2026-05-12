using Ardalis.Specification;

using SensorX.Warehouse.Domain.StrongIDs;
namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;

public class GetInventoryItemByProductIds : Specification<InventoryItem>
{
    public GetInventoryItemByProductIds(WarehouseId warehouseId, List<Guid> productIds)
    {
        Query.Where(x => x.WarehouseItemLocation.WarehouseId == warehouseId && productIds.Contains(x.ProductId.Value));
    }
}

