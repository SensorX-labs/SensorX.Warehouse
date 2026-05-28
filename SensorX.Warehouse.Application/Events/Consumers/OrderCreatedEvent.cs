
using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("order-created")]
[EntityName("order-created")]
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid NearestWarehouseId { get; init; }
    public Guid PickingNoteId { get; init; }
    public string OrderCode { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    // DeliveryInfo fields
    public string ReceiverName { get; init; } = string.Empty;
    public string ReceiverPhone { get; init; } = string.Empty;
    public string DeliveryAddress { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string TaxCode { get; init; } = string.Empty;
    public List<OrderLineItemDto> LineItems { get; init; } = [];
}

public record OrderLineItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    string ManufactureName
);
