using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;

public class StockIn : Entity<StockInId>, IAggregateRoot, ICreationTrackable
{
    private StockIn() : base() { }

    public StockIn(
        StockInId id,
        Code code,
        Code? transferOrderCode,
        string? description,
        DateTimeOffset receivedDate,
        string createdBy,
        string deliveredBy,
        string warehouseKeeper
    ) : base(id)
    {
        Code = code;
        TransferOrderCode = transferOrderCode;
        Description = description;
        ReceivedDate = receivedDate;
        CreatedBy = createdBy;
        DeliveredBy = deliveredBy;
        WarehouseKeeper = warehouseKeeper;
    }

    public Code Code { get; private set; } = null!;
    public Code? TransferOrderCode { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset ReceivedDate { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public string DeliveredBy { get; private set; } = null!;
    public string WarehouseKeeper { get; private set; } = null!;
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