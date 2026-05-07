using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;

public class StockAdjustment : Entity<StockAdjustmentId>, IAggregateRoot, ICreationTrackable
{
    private StockAdjustment() : base() { }

    public StockAdjustment(
        StockAdjustmentId id,
        Code code,
        string reason,
        string? description
    ) : base(id)
    {
        Code = code;
        Reason = reason;
        Description = description;
    }

    public Code Code { get; private set; } = null!;
    public string Reason { get; private set; } = null!;
    public string? Description { get; private set; }
    public AdjustmentStatus Status { get; private set; } = AdjustmentStatus.Pending;
    public string? RejectReason { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? RejectedAt { get; private set; }

    public WarehouseId WarehouseId { get; private set; } = WarehouseId.Default;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    private readonly List<StockAdjustmentItem> _items = [];
    public IReadOnlyList<StockAdjustmentItem> Items => _items.AsReadOnly();

    public void AddItem(ProductId productId, Code productCode, string productName, string unit, int adjustedQuantity, string? note)
    {
        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.AdjustQuantity(adjustedQuantity);
        }
        else
        {
            _items.Add(new StockAdjustmentItem(StockAdjustmentItemId.New(), productId, productCode, productName, unit, adjustedQuantity, note));
        }
    }

    public void Approve()
    {
        if (Status != AdjustmentStatus.Pending)
            throw new DomainException($"Cannot approve adjustment in status {Status}");

        if (_items.Count == 0)
            throw new DomainException("Cannot approve adjustment with no items");

        Status = AdjustmentStatus.Approved;
        ApprovedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != AdjustmentStatus.Pending)
            throw new DomainException($"Cannot reject adjustment in status {Status}");

        Status = AdjustmentStatus.Rejected;
        RejectReason = reason;
        RejectedAt = DateTimeOffset.UtcNow;
    }
}
