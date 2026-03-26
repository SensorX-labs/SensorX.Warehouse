using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.Aggregates;

public class StockOutTests
{
    /// <summary>
    /// Kiểm tra khởi tạo StockOut thành công với các thông tin cơ bản.
    /// </summary>
    [Fact]
    public void StockOut_ShouldInitializeCorrectly()
    {
        // Arrange
        var id = StockOutId.New();
        var code = Code.Create("PX");
        var description = "Xuất kho bán hàng";
        var receiverName = "Nguyễn Văn A";
        var receiverPhone = "0901234567";
        var deliveryAddress = "123 Đường ABC, Hà Nội";
        var companyName = "Công ty TNHH Một Thành Viên";
        var taxCode = "0101234567";

        // Act
        var stockOut = new StockOut(
            id,
            code,
            description,
            receiverName,
            receiverPhone,
            deliveryAddress,
            companyName,
            taxCode
        );

        // Assert
        Assert.Equal(id, stockOut.Id);
        Assert.Equal(code, stockOut.Code);
        Assert.Equal(description, stockOut.Description);
        Assert.Equal(receiverName, stockOut.RecceiverName);
        Assert.Equal(receiverPhone, stockOut.ReceiverPhone);
        Assert.Equal(deliveryAddress, stockOut.DeliveryAddress);
        Assert.Equal(companyName, stockOut.CompanyName);
        Assert.Equal(taxCode, stockOut.TaxCode);
        Assert.Equal(WarehouseId.Default, stockOut.WarehouseId);
        Assert.Null(stockOut.PickingNoteId);
        Assert.Empty(stockOut.LineItems);
    }

    /// <summary>
    /// Kiểm tra thêm sản phẩm mới vào phiếu xuất kho.
    /// </summary>
    [Fact]
    public void AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var stockOut = CreateDefaultStockOut();
        var productId = ProductId.New();
        var productCode = "P001";
        var productName = "Sản phẩm 1";
        var unit = "Cái";
        var quantity = new Quantity(10);
        var manufactureName = "Nhà sản xuất A";
        var note = "Ghi chú sản phẩm";

        // Act
        stockOut.AddItem(productId, productCode, productName, unit, quantity, manufactureName, note);

        // Assert
        Assert.Single(stockOut.LineItems);
        var item = stockOut.LineItems[0];
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(productCode, item.ProductCode);
        Assert.Equal(productName, item.ProductName);
        Assert.Equal(unit, item.Unit);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(manufactureName, item.ManufactureName);
        Assert.Equal(note, item.Note);
    }

    /// <summary>
    /// Kiểm tra cộng dồn số lượng khi thêm sản phẩm đã tồn tại trong phiếu xuất kho.
    /// </summary>
    [Fact]
    public void AddItem_ShouldUpdateQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var stockOut = CreateDefaultStockOut();
        var productId = ProductId.New();
        stockOut.AddItem(productId, "P001", "Sản phẩm 1", "Cái", new Quantity(10), "NSX A", "Note 1");

        // Act
        stockOut.AddItem(productId, "P001", "Sản phẩm 1", "Cái", new Quantity(5), "NSX A", "Note 2");

        // Assert
        Assert.Single(stockOut.LineItems);
        Assert.Equal(new Quantity(15), stockOut.LineItems[0].Quantity);
    }

    /// <summary>
    /// Kiểm tra gán mã phiếu soạn hàng (PickingNoteId) vào phiếu xuất kho.
    /// </summary>
    [Fact]
    public void SetPickingNoteId_ShouldUpdatePickingNoteId()
    {
        // Arrange
        var stockOut = CreateDefaultStockOut();
        var pickingNoteId = PickingNoteId.New();

        // Act
        stockOut.SetPickingNoteId(pickingNoteId);

        // Assert
        Assert.Equal(pickingNoteId, stockOut.PickingNoteId);
    }

    /// <summary>
    /// Hàm hỗ trợ tạo StockOut mặc định để test.
    /// </summary>
    private static StockOut CreateDefaultStockOut()
    {
        return new StockOut(
            StockOutId.New(),
            Code.Create("PX"),
            "Description",
            "Receiver",
            "0000000000",
            "Address",
            "Company",
            "TaxCode"
        );
    }
}
