using Ardalis.Specification;

using SensorX.Warehouse.Domain.StrongIDs;
namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;

public class GetInventoryItemByProductIds : Specification<InventoryItem>
{
    public GetInventoryItemByProductIds(WarehouseId warehouseId, List<ProductId> productIds)
    {
        Query.Where(x => x.WarehouseItemLocation != null && x.WarehouseItemLocation.WarehouseId == warehouseId && productIds.Contains(x.ProductId));
    }
}

