using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services.DTOs;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Domain.Services;

#pragma warning disable CA1822 // Mark members as static
public class InventoryService
{
    // Resolves one InventoryItem per ProductId from a list.
    // Throws DomainException if none found or if multiple locations exist for same product.
    private static InventoryItem ResolveItem(List<InventoryItem> items, ProductId productId, Code productCode)
    {
        var matches = items.Where(x => x.ProductId == productId).ToList();
        if (matches.Count == 0)
            throw new DomainException($"Inventory item not found for product {productCode}");
        if (matches.Count > 1)
            throw new DomainException($"Multiple inventory items found for product {productCode} — cannot resolve unambiguously. Ensure each product has a single warehouse entry.");
        return matches[0];
    }
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
            note.WarehouseId,
            Code.Create("PX"),
            note.Description,
            note.DeliveryInfo
        );
        stockOut.SetPickingNoteId(note.Id);

        var itemsByProduct = items.ToLookup(x => x.ProductId);

        foreach (var item in note.LineItems)
        {
            var inventoryItem = ResolveItem(items, item.ProductId, item.ProductCode);
            stockOut.AddItem(item.ProductId, item.ProductCode, item.ProductName, item.Unit, item.Quantity, item.ManufactureName, item.Note);
            inventoryItem.CancelAllocation(item.Quantity);
            inventoryItem.ConfirmStockOut(item.Quantity);
        }

        return stockOut;
    }

    /// <summary>
    /// Tạo phiếu nhập kho (StockIn) và cập nhật số lượng tồn kho vật lý.
    /// </summary>
    public StockIn CreateStockIn(
        WarehouseId warehouseId,
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
            warehouseId,
            Code.Create("PN"),
            transferOrderCode,
            description,
            receivedDate,
            createdBy,
            deliveredBy,
            warehouseKeeper
        );
        foreach (var item in lineItems)
        {
            var inventoryItem = ResolveItem(items, item.ProductId, item.ProductCode);
            stockIn.AddItem(item.ProductId, item.ProductCode, item.ProductName, item.Unit, item.Quantity);
            inventoryItem.ConfirmStockIn(item.Quantity);
        }

        return stockIn;
    }

    /// <summary>
    /// Điều chỉnh kho (xuất kho trực tiếp)
    /// </summary>
    public StockOut AdjustInventory(WarehouseId warehouseId, InventoryItem inventoryItem, StockOutLineRequest lineItem, string reason)
    {
        var stockOut = new StockOut(
            StockOutId.New(),
            warehouseId,
            Code.Create("PX"),
            reason,
            null
        );
        stockOut.AddItem(
            lineItem.ProductId,
            lineItem.ProductCode,
            lineItem.ProductName,
            lineItem.Unit,
            lineItem.Quantity,
            lineItem.ManufactureName,
            lineItem.Note
        );
        inventoryItem.ConfirmStockOut(lineItem.Quantity);
        return stockOut;
    }

    /// <summary>
    /// Bắt đầu quá trình lấy hàng, thực hiện giữ hàng (Allocate) trong kho.
    /// </summary>
    public void StartPicking(List<InventoryItem> items, PickingNote pickingNote)
    {
        pickingNote.StartPicking();

        foreach (var item in pickingNote.LineItems)
        {
            var inventoryItem = ResolveItem(items, item.ProductId, item.ProductCode);
            inventoryItem.Allocate(item.Quantity);
        }
    }

    /// <summary>
    /// Hủy quá trình lấy hàng, thực hiện giải phóng hàng giữ (CancelAllocation) trong kho.
    /// </summary>
    public void CancelPicking(List<InventoryItem> items, PickingNote pickingNote)
    {
        pickingNote.ConfirmCanceled();

        foreach (var item in pickingNote.LineItems)
        {
            var inventoryItem = ResolveItem(items, item.ProductId, item.ProductCode);
            inventoryItem.CancelAllocation(item.Quantity);
        }
    }


}
#pragma warning restore CA1822

