using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.Services.DTOs;

public class StockOutLineRequest
{
    public required ProductId ProductId { get; set; }
    public required Code ProductCode { get; set; }
    public required string ProductName { get; set; }
    public required string Unit { get; set; }
    public required Quantity Quantity { get; set; }
    public required string ManufactureName { get; set; }
    public string? Note { get; set; }
}