using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.Common.Interfaces;

public interface IWarehouseProvider
{
    WarehouseId GetCurrentWarehouseId();
}