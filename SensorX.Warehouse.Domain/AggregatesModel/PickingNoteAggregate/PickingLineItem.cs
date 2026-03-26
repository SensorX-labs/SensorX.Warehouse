using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public class PickingLineItem(
    PickingLineItemId pickingLineItemId,
    ProductId productId,
    Quantity requestedQuantity
) : Entity<PickingLineItemId>(pickingLineItemId)
{
    public ProductId ProductId { get; private set; } = productId;
    public Quantity RequestedQuantity { get; private set; } = requestedQuantity;

    public void AddQuantity(Quantity quantity)
    {
        RequestedQuantity += quantity;
    }
}