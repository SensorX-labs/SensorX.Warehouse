using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public class PickingNote : Entity<PickingNoteId>, IAggregateRoot, ICreationTrackable
{
    public Code Code { get; private set; } = null!;
    public DocumentReference SourceDocument { get; private set; } = null!;
    public PickingStatus Status { get; private set; }
    public string? Description { get; private set; }
    public DeliveryInfo DeliveryInfo { get; private set; } = null!;

    private readonly List<PickingLineItem> _lineItems = [];
    public IReadOnlyList<PickingLineItem> LineItems => _lineItems.AsReadOnly();

    public WarehouseId WarehouseId { get; init; } = WarehouseId.Default;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    private PickingNote() : base() { }

    private PickingNote(
        PickingNoteId id,
        Code code,
        DocumentReference sourceDocument,
        PickingStatus status,
        string? description,
        DeliveryInfo deliveryInfo
    ) : base(id)
    {
        Code = code;
        SourceDocument = sourceDocument;
        Status = status;
        Description = description;
        DeliveryInfo = deliveryInfo;
    }

    public static PickingNote CreateForSalesOrder(OrderId orderId, Code noteCode, string? description, DeliveryInfo deliveryInfo)
    {
        return new PickingNote(
            PickingNoteId.New(),
            noteCode,
            new DocumentReference(DocumentType.SalesOrder, orderId, noteCode),
            PickingStatus.Pending,
            description,
            deliveryInfo
        );
    }

    public static PickingNote CreateForTransferOrder(TransferOrderId transferOrderId, Code noteCode, string? description, DeliveryInfo deliveryInfo)
    {
        return new PickingNote(
            PickingNoteId.New(),
            noteCode,
            new DocumentReference(DocumentType.TransferOrder, transferOrderId, noteCode),
            PickingStatus.Pending,
            description,
            deliveryInfo
        );
    }

    public void AddItem(ProductId productId, Code productCode, string productName, string unit, Quantity quantity, string manufactureName, string note)
    {
        var existingItem = _lineItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            _lineItems.Add(new PickingLineItem(PickingLineItemId.New(), productId, productCode, productName, unit, quantity, manufactureName, note));
        }
    }

    public void StartPicking() => Status = PickingStatus.Picking;

    public void ConfirmCanceled() => Status = PickingStatus.Canceled;

    public void ConfirmCompleted() => Status = PickingStatus.Completed;
}