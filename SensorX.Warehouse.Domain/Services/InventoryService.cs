using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.Services.DTOs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.Services;

public class InventoryService
{
    public StockOut CreateStockOutFromPickingNote(
        List<InventoryItem> items,
        PickingNote note
    )
    {
        var stockOut = new StockOut(
            StockOutId.New(),
            Code.Create("PX"),
            note.Description,
            note.DeliveryInfo
        );
        stockOut.SetPickingNoteId(note.Id);

        var InventoryItems = items.ToDictionary(x => x.ProductId);
        foreach (var item in note.LineItems)
        {
            stockOut.AddItem(item.ProductId, item.ProductCode, item.ProductName, item.Unit, item.Quantity, item.ManufactureName, item.Note);

            if (!InventoryItems.TryGetValue(item.ProductId, out var inventoryItem))
            {
                throw new DomainException($"Inventory item not found for product {item.ProductCode}");
            }
            inventoryItem.CancelAllocation(item.Quantity);
            inventoryItem.ConfirmStockOut(item.Quantity);
        }

        return stockOut;
    }

    public StockIn CreateStockIn(
        List<InventoryItem> items,
        List<StockInLineRequest> lineItems,
        string? transferOrderCode,
        string description,
        DateTimeOffset receivedDate,
        string createdBy,
        string deliveredBy,
        string warehouseKeeper
    )
    {
        var stockIn = new StockIn(
            StockInId.New(),
            Code.Create("PN"),
            transferOrderCode,
            description,
            receivedDate,
            createdBy,
            deliveredBy,
            warehouseKeeper
        );

        var InventoryItems = items.ToDictionary(x => x.ProductId);
        foreach (var item in lineItems)
        {
            stockIn.AddItem(item.ProductId, item.ProductCode, item.ProductName, item.Unit, item.Quantity);

            if (!InventoryItems.TryGetValue(item.ProductId, out var inventoryItem))
            {
                throw new DomainException($"Inventory item not found for product {item.ProductCode}");
            }
            inventoryItem.ConfirmStockIn(item.Quantity);
        }

        return stockIn;
    }

    public StockOut AdjustInventory(List<InventoryItem> items, List<StockOutLineRequest> lineItems)
    {
        var stockOut = new StockOut(
            StockOutId.New(),
            Code.Create("PX"),
            "Điều chỉnh tồn kho",
            null
        );

        var InventoryItems = items.ToDictionary(x => x.ProductId);
        foreach (var item in lineItems)
        {
            stockOut.AddItem(item.ProductId, item.ProductCode, item.ProductName, item.Unit, item.Quantity, item.ManufactureName, item.Note);

            if (!InventoryItems.TryGetValue(item.ProductId, out var inventoryItem))
            {
                throw new DomainException($"Inventory item not found for product {item.ProductCode}");
            }
            inventoryItem.ConfirmStockOut(item.Quantity);
        }

        return stockOut;
    }

    public void StartPicking(List<InventoryItem> items, PickingNote pickingNote)
    {
        pickingNote.StartPicking();

        var InventoryItems = items.ToDictionary(x => x.ProductId);
        foreach (var item in pickingNote.LineItems)
        {
            if (!InventoryItems.TryGetValue(item.ProductId, out var inventoryItem))
            {
                throw new DomainException($"Inventory item not found for product {item.ProductCode}");
            }
            inventoryItem.Allocate(item.Quantity);
        }
    }
}