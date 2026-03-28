using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

public class StockInItem : Entity<StockInItemId>
{
    private StockInItem() : base() { }

    public StockInItem(
        StockInItemId id,
        ProductId productId,
        Code productCode,
        string productName,
        string unit,
        Quantity quantity
    ) : base(id)
    {
        ProductId = productId;
        ProductCode = productCode;
        ProductName = productName;
        Unit = unit;
        Quantity = quantity;
    }

    public ProductId ProductId { get; private set; } = null!;
    public Code ProductCode { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;

    public void AddQuantity(Quantity quantity) => Quantity += quantity;
}