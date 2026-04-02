using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

using SensorX.Warehouse.Domain.StrongIDs;
namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

public class InventoryItem : Entity<InventoryItemId>, IAggregateRoot, ICreationTrackable, IUpdateTrackable
{
    private InventoryItem() : base() { }

    public InventoryItem(
        InventoryItemId id,
        ProductId productId,
        WarehouseItemLocation? warehouseItemLocation,
        Quantity physicalQuantity,
        Quantity allocatedQuantity
    ) : base(id)
    {
        ProductId = productId;
        WarehouseItemLocation = warehouseItemLocation;
        PhysicalQuantity = physicalQuantity;
        AllocatedQuantity = allocatedQuantity;
    }

    public ProductId ProductId { get; private set; } = null!;
    public Quantity PhysicalQuantity { get; private set; } = null!;
    public Quantity AllocatedQuantity { get; private set; } = null!;
    public WarehouseItemLocation? WarehouseItemLocation { get; private set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public void Allocate(Quantity quantity)
    {
        if (quantity > GetSalableQuantity())
        {
            throw new DomainException("Not enough stock to allocate");
        }
        AllocatedQuantity += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void CancelAllocation(Quantity quantity)
    {
        if (quantity > AllocatedQuantity)
        {
            throw new DomainException("Not enough allocated stock to cancel");
        }
        AllocatedQuantity -= quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ConfirmStockOut(Quantity quantity)
    {
        if (quantity > PhysicalQuantity)
        {
            throw new DomainException("Not enough stock to confirm stock out");
        }
        PhysicalQuantity -= quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ConfirmStockIn(Quantity quantity)
    {
        PhysicalQuantity += quantity;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public int GetSalableQuantity()
    {
        return PhysicalQuantity - AllocatedQuantity;
    }
}

