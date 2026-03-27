using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public class PickingLineItem(
    PickingLineItemId pickingLineItemId,
    ProductId productId,
    string productCode,
    string productName,
    string unit,
    Quantity quantity,
    string manufactureName,
    string note
) : Entity<PickingLineItemId>(pickingLineItemId)
{
    public ProductId ProductId { get; private set; } = productId;
    public string ProductCode { get; private set; } = productCode;
    public string ProductName { get; private set; } = productName;
    public string Unit { get; private set; } = unit;
    public Quantity Quantity { get; private set; } = quantity;
    public string ManufactureName { get; private set; } = manufactureName;
    public string Note { get; private set; } = note;

    public void AddQuantity(Quantity quantity) => Quantity += quantity;
}