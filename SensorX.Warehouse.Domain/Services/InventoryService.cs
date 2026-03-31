using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.Services.DTOs;
using SensorX.Warehouse.Domain.ValueObjects;

using SensorX.Warehouse.Domain.SeedWork;
namespace SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;

#pragma warning disable CA1822 // Mark members as static
public class InventoryService
{
    /// <summary>
    /// Tạo phiếu xuất kho (StockOut) từ lệnh lấy hàng (PickingNote).
    /// Đồng thời cập nhật trạng thái giữ hàng (CancelAllocation) và xác nhận xuất kho (ConfirmStockOut) cho từng mặt hàng.
    /// </summary>
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

    /// <summary>
    /// Tạo phiếu nhập kho (StockIn) và cập nhật số lượng tồn kho vật lý.
    /// </summary>
    public StockIn CreateStockIn(
        List<InventoryItem> items,
        List<StockInLineRequest> lineItems,
        Code? transferOrderCode,
        string? description,
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

    /// <summary>
    /// Điều chỉnh kho (xuất kho trực tiếp) và cập nhật số lượng tồn kho vật lý.
    /// </summary>
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

    /// <summary>
    /// Bắt đầu quá trình lấy hàng, thực hiện giữ hàng (Allocate) trong kho.
    /// </summary>
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
#pragma warning restore CA1822

