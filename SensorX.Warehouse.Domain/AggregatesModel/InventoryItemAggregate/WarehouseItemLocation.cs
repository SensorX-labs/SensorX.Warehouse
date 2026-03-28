using SensorX.Warehouse.Domain.ValueObjects;

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