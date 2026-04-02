using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.Aggregates;

public class StockInTests
{
    /// <summary>
    /// Kiểm tra khởi tạo StockIn thành công với các thông tin cơ bản.
    /// </summary>
    [Fact]
    public void StockIn_ShouldInitializeCorrectly()
    {
        // Arrange
        var id = StockInId.New();
        var code = Code.Create("PN");
        var transferOrderCode = Code.Create("TO");
        var description = "Nhập kho từ lệnh điều chuyển";
        var receivedDate = DateTimeOffset.Now;
        var createdBy = "Admin";
        var deliveredBy = "Shipper A";
        var warehouseKeeper = "Keeper B";

        // Act
        var stockIn = new StockIn(
            id,
            code,
            transferOrderCode,
            description,
            receivedDate,
            createdBy,
            deliveredBy,
            warehouseKeeper
        );

        // Assert
        Assert.Equal(id, stockIn.Id);
        Assert.Equal(code, (string)stockIn.Code!);
        Assert.Equal(transferOrderCode, (string)stockIn.TransferOrderCode!);
        Assert.Equal(description, stockIn.Description);
        Assert.Equal(receivedDate, stockIn.ReceivedDate);
        Assert.Equal(createdBy, stockIn.CreatedBy);
        Assert.Equal(deliveredBy, stockIn.DeliveredBy);
        Assert.Equal(warehouseKeeper, stockIn.WarehouseKeeper);
        Assert.Equal(WarehouseId.Default, stockIn.WarehouseId);
        Assert.Empty(stockIn.LineItems);
    }

    /// <summary>
    /// Kiểm tra thêm sản phẩm mới vào phiếu nhập kho.
    /// </summary>
    [Fact]
    public void AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var stockIn = CreateDefaultStockIn();
        var productId = ProductId.New();
        var productName = "Sản phẩm 1";
        var unit = "Cái";
        var quantity = new Quantity(10);

        var pCode = Code.Create("P");
        stockIn.AddItem(productId, pCode, productName, unit, quantity);

        // Assert
        Assert.Single(stockIn.LineItems);
        var item = stockIn.LineItems[0];
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(pCode, item.ProductCode);
        Assert.Equal(productName, item.ProductName);
        Assert.Equal(unit, item.Unit);
        Assert.Equal(quantity, item.Quantity);
    }

    /// <summary>
    /// Kiểm tra cộng dồn số lượng khi thêm sản phẩm đã tồn tại trong phiếu nhập kho.
    /// </summary>
    [Fact]
    public void AddItem_ShouldUpdateQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var stockIn = CreateDefaultStockIn();
        var productId = ProductId.New();
        stockIn.AddItem(productId, Code.Create("P"), "Sản phẩm 1", "Cái", new Quantity(10));

        // Act
        stockIn.AddItem(productId, Code.Create("P"), "Sản phẩm 1", "Cái", new Quantity(5));

        // Assert
        Assert.Single(stockIn.LineItems);
        Assert.Equal(new Quantity(15), stockIn.LineItems[0].Quantity);
    }

    /// <summary>
    /// Hàm hỗ trợ tạo StockIn mặc định để test.
    /// </summary>
    private static StockIn CreateDefaultStockIn()
    {
        return new StockIn(
            StockInId.New(),
            Code.Create("PN"),
            Code.Create("TO"),
            "Description",
            DateTimeOffset.Now,
            "Creator",
            "Deliverer",
            "Keeper"
        );
    }
}
