using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

using SensorX.Warehouse.Domain.StrongIDs;
namespace SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;

public class StockOut : Entity<StockOutId>, IAggregateRoot, ICreationTrackable
{
    private StockOut() : base() { }

    public StockOut(
        StockOutId id,
        WarehouseId warehouseId,
        Code code,
        string? description, // lý do xuất
        DeliveryInfo? deliveryInfo
    ) : base(id)
    {
        WarehouseId = warehouseId;
        Code = code;
        Description = description;
        DeliveryInfo = deliveryInfo;
    }

    public Code Code { get; private set; } = null!;
    public string? Description { get; private set; } // lý do xuất
    public DeliveryInfo? DeliveryInfo { get; private set; }

    public WarehouseId WarehouseId { get; private set; } = null!;
    public PickingNoteId? PickingNoteId { get; private set; } // null thì xuất theo điều chỉnh tồn kho

    private readonly List<StockOutItem> _lineItems = [];
    public IReadOnlyList<StockOutItem> LineItems => _lineItems.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void AddItem(ProductId productId, Code productCode, string productName, string unit, Quantity quantity, string manufactureName, string? note)
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


