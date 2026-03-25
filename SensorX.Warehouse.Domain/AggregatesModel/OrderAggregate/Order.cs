using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.OrderAggregate;

public class Order : Entity<OrderId>, IAggregateRoot
{
    public CustomerId CustomerId { get; init; }
    public CustomerInfo CustomerInfo { get; init; }
    public OrderStatus OrderStatus { get; init; }
    public ShippingAddress ShippingAddress { get; init; }
    public decimal TotalAmount { get; init; }

    public Address address { };

    public readonly List<OrderItem> OrderItems = [];
    public Order(OrderId id) : base(id)
    {
    }
}