using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;

public class StockOut(
    StockOutId id,
    Code code,
    string description,
    string recceiverName,
    string receiverPhone,
    string deliveryAddress,
    string companyName,
    string taxCode
) : Entity<StockOutId>(id), IAggregateRoot
{
    public Code Code { get; private set; } = code;
    public string Description { get; private set; } = description;
    public string RecceiverName { get; private set; } = recceiverName;
    public string ReceiverPhone { get; private set; } = receiverPhone;
    public string DeliveryAddress { get; private set; } = deliveryAddress;
    public string CompanyName { get; private set; } = companyName;
    public string TaxCode { get; private set; } = taxCode;

    public WarehouseId WarehouseId { get; private set; } = WarehouseId.Default;
    public PickingNoteId? PickingNoteId { get; private set; }

    private readonly List<StockOutItem> _lineItems = [];
    public IReadOnlyList<StockOutItem> LineItems => _lineItems.AsReadOnly();

    public void AddItem(ProductId productId, string productCode, string productName, string unit, Quantity quantity, string manufactureName, string note)
    {
        var existingItem = _lineItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            _lineItems.Add(new StockOutItem(StockOutItemId.New(), productId, productCode, productName, unit, quantity, manufactureName, note));
        }
    }

    public void SetPickingNoteId(PickingNoteId pickingNoteId) => PickingNoteId = pickingNoteId;
}
