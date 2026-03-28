using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.Services.DTOs;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.Services;

public class InventoryServiceTests
{
    private readonly InventoryService _inventoryService;

    public InventoryServiceTests()
    {
        _inventoryService = new InventoryService();
    }

    private InventoryItem CreateSampleInventoryItem(ProductId productId, int physical, int allocated)
    {
        return new InventoryItem(
            InventoryItemId.New(),
            productId,
            new WarehouseItemLocation("Kho Hải Phòng", "1", "A", "R01"),
            new Quantity(physical),
            new Quantity(allocated)
        );
    }

    /// <summary>
    /// Kiểm tra tạo phiếu xuất kho từ lệnh lấy hàng và cập nhật tồn kho (giảm Allocated và giảm Physical).
    /// </summary>
    [Fact]
    public void CreateStockOutFromPickingNote_ShouldCreateStockOutAndAdjustInventory()
    {
        // Arrange
        var productId = ProductId.New();
        var productCode = Code.Create("P");
        var quantity = new Quantity(5);

        var inventoryItem = CreateSampleInventoryItem(productId, 10, 5); // 10 physical, 5 allocated (from picking)
        var items = new List<InventoryItem> { inventoryItem };

        var note = PickingNote.CreateForSalesOrder(OrderId.New(), Code.Create("PN"), "Note 001", new DeliveryInfo("Customer", "0987654321", "Address", "Company", "Tax123"));
        note.AddItem(productId, productCode, "Product 1", "Unit", quantity, "Manufacture", "Note");

        // Act
        var stockOut = _inventoryService.CreateStockOutFromPickingNote(items, note);

        // Assert
        Assert.NotNull(stockOut);
        Assert.Equal(note.Id, stockOut.PickingNoteId);
        Assert.Single(stockOut.LineItems);
        Assert.Equal(0, (int)inventoryItem.AllocatedQuantity); // 5 - 5 = 0
        Assert.Equal(5, (int)inventoryItem.PhysicalQuantity); // 10 - 5 = 5
    }

    /// <summary>
    /// Kiểm tra ném lỗi khi không tìm thấy item trong danh sách tồn kho khi tạo phiếu xuất.
    /// </summary>
    [Fact]
    public void CreateStockOutFromPickingNote_ShouldThrowException_WhenItemNotFound()
    {
        // Arrange
        var productId = ProductId.New();
        var items = new List<InventoryItem>(); // Empty list

        var productCode = Code.Create("P");
        var note = PickingNote.CreateForSalesOrder(OrderId.New(), Code.Create("PN"), "Note", new DeliveryInfo("Customer", "0987654321", "Address", "Company", "Tax123"));
        note.AddItem(productId, productCode, "Product 1", "Unit", new Quantity(5), "Manufacturer", "Note");

        // Act & Assert
        Assert.Throws<DomainException>(() => _inventoryService.CreateStockOutFromPickingNote(items, note));
    }

    /// <summary>
    /// Kiểm tra tạo phiếu nhập kho và tăng tồn kho vật lý.
    /// </summary>
    [Fact]
    public void CreateStockIn_ShouldCreateStockInAndIncreaseInventory()
    {
        // Arrange
        var productId = ProductId.New();
        var quantity = new Quantity(5);

        var inventoryItem = CreateSampleInventoryItem(productId, 10, 0);
        var items = new List<InventoryItem> { inventoryItem };

        var lineItems = new List<StockInLineRequest>
        {
            new() {
                ProductId = productId,
                ProductCode = Code.Create("P"),
                ProductName = "Product 1",
                Unit = "Unit",
                Quantity = quantity
            }
        };

        // Act
        var stockIn = _inventoryService.CreateStockIn(
            items,
            lineItems,
            Code.Create("TO"),
            "Description",
            DateTimeOffset.Now,
            "Creator",
            "Deliverer",
            "Keeper"
        );

        // Assert
        Assert.NotNull(stockIn);
        Assert.Single(stockIn.LineItems);
        Assert.Equal(15, (int)inventoryItem.PhysicalQuantity); // 10 + 5 = 15
    }

    /// <summary>
    /// Kiểm tra ném lỗi khi không tìm thấy item trong danh sách tồn kho khi tạo phiếu nhập.
    /// </summary>
    [Fact]
    public void CreateStockIn_ShouldThrowException_WhenItemNotFound()
    {
        // Arrange
        var productId = ProductId.New();
        var items = new List<InventoryItem>();
        var lineItems = new List<StockInLineRequest>
        {
            new() {
                ProductId = productId,
                ProductCode = Code.Create("P"),
                ProductName = "Product 1",
                Unit = "Unit",
                Quantity = new Quantity(5)
            }
        };

        // Act & Assert
        Assert.Throws<DomainException>(() => _inventoryService.CreateStockIn(items, lineItems, Code.Create("TO"), "Desc", DateTimeOffset.Now, "C", "D", "K"));
    }

    /// <summary>
    /// Kiểm tra điều chỉnh tồn kho (xuất kho trực tiếp) và giảm tồn kho vật lý.
    /// </summary>
    [Fact]
    public void AdjustInventory_ShouldCreateStockOutAndDecreaseInventory()
    {
        // Arrange
        var productId = ProductId.New();
        var quantity = new Quantity(5);

        var inventoryItem = CreateSampleInventoryItem(productId, 10, 0);
        var items = new List<InventoryItem> { inventoryItem };

        var lineItems = new List<StockOutLineRequest>
        {
            new() {
                ProductId = productId,
                ProductCode = Code.Create("P"),
                ProductName = "Product 1",
                Unit = "Unit",
                Quantity = quantity,
                ManufactureName = "Manufacturer",
                Note = "Note"
            }
        };

        // Act
        var stockOut = _inventoryService.AdjustInventory(items, lineItems);

        // Assert
        Assert.NotNull(stockOut);
        Assert.Single(stockOut.LineItems);
        Assert.Equal(5, (int)inventoryItem.PhysicalQuantity); // 10 - 5 = 5
    }

    /// <summary>
    /// Kiểm tra bắt đầu lấy hàng: Tăng số lượng đã giữ (Allocated Quantity).
    /// </summary>
    [Fact]
    public void StartPicking_ShouldAllocateInventory()
    {
        // Arrange
        var productId = ProductId.New();
        var productCode = Code.Create("P");
        var quantity = new Quantity(5);

        var inventoryItem = CreateSampleInventoryItem(productId, 10, 0);
        var items = new List<InventoryItem> { inventoryItem };

        var note = PickingNote.CreateForSalesOrder(OrderId.New(), Code.Create("PN"), "Note", new DeliveryInfo("Customer", "0987654321", "Address", "Company", "Tax123"));
        note.AddItem(productId, productCode, "Product 1", "Unit", quantity, "Manufacturer", "Note");

        // Act
        _inventoryService.StartPicking(items, note);

        // Assert
        Assert.Equal(5, (int)inventoryItem.AllocatedQuantity);
        Assert.Equal(10, (int)inventoryItem.PhysicalQuantity); // Physical quantity should not change yet
    }
}
