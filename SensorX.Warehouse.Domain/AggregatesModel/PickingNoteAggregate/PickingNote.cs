using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public class PickingNote(
    PickingNoteId pickingNoteId,
    Code code,
    DocumentReference sourceDocument,
    PickingStatus status,
    string description,
    DeliveryInfo deliveryInfo
) : Entity<PickingNoteId>(pickingNoteId), IAggregateRoot
{
    public Code Code { get; private set; } = code;
    public DocumentReference SourceDocument { get; private set; } = sourceDocument;
    public PickingStatus Status { get; private set; } = status;
    public string Description { get; private set; } = description;
    public DeliveryInfo DeliveryInfo { get; private set; } = deliveryInfo;
    public WarehouseId WarehouseId { get; private set; } = WarehouseId.Default;

    private readonly List<PickingLineItem> _lineItems = [];
    public IReadOnlyList<PickingLineItem> LineItems => _lineItems.AsReadOnly();

    public static PickingNote CreateForSalesOrder(Guid orderId, string noteCode, string description, DeliveryInfo deliveryInfo)
    {
        return new PickingNote(
            PickingNoteId.New(),
            Code.From(noteCode),
            new DocumentReference(DocumentType.SalesOrder, orderId, noteCode),
            PickingStatus.Pending,
            description,
            deliveryInfo
        );
    }

    public static PickingNote CreateForTransferOrder(Guid transferOrderId, string noteCode, string description, DeliveryInfo deliveryInfo)
    {
        return new PickingNote(
            PickingNoteId.New(),
            Code.From(noteCode),
            new DocumentReference(DocumentType.TransferOrder, transferOrderId, noteCode),
            PickingStatus.Pending,
            description,
            deliveryInfo
        );
    }

    public void AddItem(ProductId productId, string productCode, string productName, string unit, Quantity quantity, string manufactureName, string note)
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