using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;

public record StockAdjustmentItemId(Guid Value) : EntityId<StockAdjustmentItemId>(Value)
{
    public static StockAdjustmentItemId New() => new(Guid.CreateVersion7());
}

public class StockAdjustmentItem : Entity<StockAdjustmentItemId>
{
    private StockAdjustmentItem() : base() { }

    public StockAdjustmentItem(
        StockAdjustmentItemId id,
        ProductId productId,
        Code productCode,
        string productName,
        string unit,
        int adjustedQuantity,
        string? note
    ) : base(id)
    {
        ProductId = productId;
        ProductCode = productCode;
        ProductName = productName;
        Unit = unit;
        AdjustedQuantity = adjustedQuantity;
        Note = note;
    }

    public ProductId ProductId { get; private set; } = null!;
    public Code ProductCode { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string Unit { get; private set; } = null!;

    /// <summary>
    /// Số lượng điều chỉnh. Dương = tăng, Âm = giảm.
    /// </summary>
    public int AdjustedQuantity { get; private set; }

    public string? Note { get; private set; }

    public void AdjustQuantity(int delta) => AdjustedQuantity += delta;
}
