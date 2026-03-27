using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

public class InventoryItem(
    InventoryItemId inventoryItemId,
    ProductId productId,
    WarehouseId warehouseId,
    WarehouseItemLocation warehouseItemLocation,
    Quantity physicalQuantity,
    Quantity allocatedQuantity
) : Entity<InventoryItemId>(inventoryItemId), IAggregateRoot, ICreationTrackable, IUpdateTrackable
{
    public ProductId ProductId { get; private set; } = productId;
    public WarehouseId WarehouseId { get; private set; } = warehouseId;
    public WarehouseItemLocation WarehouseItemLocation { get; private set; } = warehouseItemLocation;
    public Quantity PhysicalQuantity { get; private set; } = physicalQuantity;
    public Quantity AllocatedQuantity { get; private set; } = allocatedQuantity;

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