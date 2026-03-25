using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

public record WarehouseItemLocation(
    WarehouseId WarehouseId,
    string WarehouseName,
    string Floor,
    string BrandZone,
    string RackCode
);