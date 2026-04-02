using SensorX.Warehouse.Domain.ValueObjects;

using SensorX.Warehouse.Domain.StrongIDs;
namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

public record WarehouseItemLocation(
    string WarehouseName,
    string Floor,
    string BrandZone,
    string RackCode
)
{
    public WarehouseId WarehouseId { get; init; } = WarehouseId.Default;
}

