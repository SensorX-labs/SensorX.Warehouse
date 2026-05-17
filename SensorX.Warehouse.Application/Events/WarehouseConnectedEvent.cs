namespace SensorX.Warehouse.Application.Events;

public record WarehouseConnectedEvent
(
    string WarehouseId,
    string WarehouseName,
    string Status,
    DateTimeOffset Ts
);
