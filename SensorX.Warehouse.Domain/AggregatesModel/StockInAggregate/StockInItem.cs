using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

public class StockInItem(
    StockInItemId id,
    ProductId productId,
    string productCode,
    string productName,
    string unit,
    Quantity quantity
) : Entity<StockInItemId>(id)
{
    public ProductId ProductId { get; private set; } = productId;
    public string ProductCode { get; private set; } = productCode;
    public string ProductName { get; private set; } = productName;
    public string Unit { get; private set; } = unit;
    public Quantity Quantity { get; private set; } = quantity;

    public void AddQuantity(Quantity quantity)
    {
        Quantity += quantity;
    }
}