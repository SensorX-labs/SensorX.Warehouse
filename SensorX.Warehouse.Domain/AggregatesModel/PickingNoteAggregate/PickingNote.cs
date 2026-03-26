using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public class PickingNote(
    PickingNoteId pickingNoteId,
    string code,
    DocumentReference sourceDocument,
    PickingStatus status
) : Entity<PickingNoteId>(pickingNoteId), IAggregateRoot
{
    public string Code { get; private set; } = code;
    public DocumentReference SourceDocument { get; private set; } = sourceDocument;
    public PickingStatus Status { get; private set; } = status;

    private readonly List<PickingLineItem> _lineItems = [];
    public IReadOnlyList<PickingLineItem> LineItems => _lineItems.AsReadOnly();

    public static PickingNote CreateForSalesOrder(Guid orderId, string noteCode)
    {
        return new PickingNote(
            PickingNoteId.New(),
            noteCode,
            new DocumentReference(DocumentType.SalesOrder, orderId, noteCode),
            PickingStatus.Pending
        );
    }

    public static PickingNote CreateForTransferOrder(Guid transferOrderId, string noteCode)
    {
        return new PickingNote(
            PickingNoteId.New(),
            noteCode,
            new DocumentReference(DocumentType.TransferOrder, transferOrderId, noteCode),
            PickingStatus.Pending
        );
    }

    public void AddItem(ProductId productId, Quantity quantity)
    {
        var existingItem = _lineItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            _lineItems.Add(new PickingLineItem(PickingLineItemId.New(), productId, quantity));
        }
    }

    public void StartPicking() => Status = PickingStatus.Picking;

    public void ConfirmCanceled() => Status = PickingStatus.Canceled;

    public void ConfirmCompleted() => Status = PickingStatus.Completed;
}