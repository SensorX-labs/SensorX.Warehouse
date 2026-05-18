namespace SensorX.Warehouse.Application.Events;

public record WarehouseConnectedEvent
(
    string WarehouseId,
    string WarehouseName,
    string WarehouseAddress,
    string Status,
    DateTimeOffset Ts
);
