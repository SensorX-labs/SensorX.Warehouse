using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.OrderAggregate;

public record OrderItem : Entity<OrderItemId>
{
    public string ProductName { get; private set; }
    public string ProductCode { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem(
        string productName,
        string productCode,
        decimal unitPrice,
        int quantity
    ) : base(new OrderItemId(Guid.CreateVersion7()))
    {
        ProductName = productName;
        ProductCode = productCode;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public static OrderItem Create(
        string productName,
        string productCode,
        decimal unitPrice,
        int quantity
    )
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name cannot be null or empty");
        }
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code cannot be null or empty");
        }
        if (unitPrice < 0)
        {
            throw new ArgumentException("Unit price cannot be negative");
        }
        if (quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative");
        }
        return new OrderItem(productName, productCode, unitPrice, quantity);
    }
}
