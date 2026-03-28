using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

public class StockIn(
    StockInId id,
    Code code,
    Code? transferOrderCode,
    string description,
    DateTimeOffset receivedDate,
    string createdBy,
    string deliveredBy,
    string warehouseKeeper
) : Entity<StockInId>(id), IAggregateRoot, ICreationTrackable
{
    public Code Code { get; private set; } = code;
    public Code? TransferOrderCode { get; private set; } = transferOrderCode;
    public string Description { get; private set; } = description;
    public DateTimeOffset ReceivedDate { get; private set; } = receivedDate;
    public string CreatedBy { get; private set; } = createdBy;
    public string DeliveredBy { get; private set; } = deliveredBy;
    public string WarehouseKeeper { get; private set; } = warehouseKeeper;
    public WarehouseId WarehouseId { get; private set; } = WarehouseId.Default;

    private readonly List<StockInItem> _lineItems = [];
    public IReadOnlyList<StockInItem> LineItems => _lineItems.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void AddItem(ProductId productId, Code productCode, string productName, string unit, Quantity quantity)
    {
        var existingItem = _lineItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            _lineItems.Add(new StockInItem(StockInItemId.New(), productId, productCode, productName, unit, quantity));
        }
    }
}