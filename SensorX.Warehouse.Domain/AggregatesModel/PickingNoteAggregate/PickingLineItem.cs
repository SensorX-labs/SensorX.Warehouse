using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public class PickingLineItem : Entity<PickingLineItemId>
{
    private PickingLineItem() : base() { }

    public PickingLineItem(
        PickingLineItemId id,
        ProductId productId,
        Code productCode,
        string productName,
        string unit,
        Quantity quantity,
        string manufactureName,
        string note
    ) : base(id)
    {
        ProductId = productId;
        ProductCode = productCode;
        ProductName = productName;
        Unit = unit;
        Quantity = quantity;
        ManufactureName = manufactureName;
        Note = note;
    }

    public ProductId ProductId { get; private set; } = null!;
    public Code ProductCode { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;
    public string ManufactureName { get; private set; } = null!;
    public string Note { get; private set; } = null!;

    public void AddQuantity(Quantity quantity) => Quantity += quantity;
}