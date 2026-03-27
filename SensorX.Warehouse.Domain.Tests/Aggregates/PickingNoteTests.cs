using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.Aggregates;

public class PickingNoteTests
{
    /// <summary>
    /// Kiểm tra khởi tạo PickingNote cho đơn bán hàng (SalesOrder) thành công.
    /// </summary>
    [Fact]
    public void CreateForSalesOrder_ShouldInitializeCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var noteCode = "PN-SO-001";
        var description = "Ghi chú soạn hàng";
        var deliveryInfo = new DeliveryInfo("Người nhận", "0901234567", "Hà Nội", "Công ty X", "010203");

        // Act
        var pickingNote = PickingNote.CreateForSalesOrder(orderId, noteCode, description, deliveryInfo);

        // Assert
        Assert.NotNull(pickingNote);
        Assert.Equal(noteCode, pickingNote.Code);
        Assert.Equal(description, pickingNote.Description);
        Assert.Equal(deliveryInfo, pickingNote.DeliveryInfo);
        Assert.Equal(PickingStatus.Pending, pickingNote.Status);
        Assert.Equal(DocumentType.SalesOrder, pickingNote.SourceDocument.Type);
        Assert.Equal(orderId, pickingNote.SourceDocument.Id);
        Assert.Equal(noteCode, pickingNote.SourceDocument.Code);
        Assert.Empty(pickingNote.LineItems);
    }

    /// <summary>
    /// Kiểm tra khởi tạo PickingNote cho lệnh điều chuyển (TransferOrder) thành công.
    /// </summary>
    [Fact]
    public void CreateForTransferOrder_ShouldInitializeCorrectly()
    {
        // Arrange
        var transferOrderId = Guid.NewGuid();
        var noteCode = "PN-TO-001";
        var description = "Điều chuyển nội bộ";
        var deliveryInfo = new DeliveryInfo("Kho B", "0987654321", "TP.HCM", "Chi nhánh 1", "001122");

        // Act
        var pickingNote = PickingNote.CreateForTransferOrder(transferOrderId, noteCode, description, deliveryInfo);

        // Assert
        Assert.NotNull(pickingNote);
        Assert.Equal(noteCode, pickingNote.Code);
        Assert.Equal(description, pickingNote.Description);
        Assert.Equal(deliveryInfo, pickingNote.DeliveryInfo);
        Assert.Equal(PickingStatus.Pending, pickingNote.Status);
        Assert.Equal(DocumentType.TransferOrder, pickingNote.SourceDocument.Type);
        Assert.Equal(transferOrderId, pickingNote.SourceDocument.Id);
        Assert.Equal(noteCode, pickingNote.SourceDocument.Code);
        Assert.Empty(pickingNote.LineItems);
    }

    /// <summary>
    /// Kiểm tra thêm sản phẩm mới vào danh sách soạn hàng (LineItems).
    /// </summary>
    [Fact]
    public void AddItem_ShouldAddNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var deliveryInfo = new DeliveryInfo("Receiver", "000", "Address", "Company", "Tax");
        var pickingNote = PickingNote.CreateForSalesOrder(Guid.NewGuid(), "PN-001", "Desc", deliveryInfo);
        var productId = ProductId.New();
        var quantity = new Quantity(10);

        // Act
        pickingNote.AddItem(productId, "P001", "Sản phẩm 1", "Cái", quantity, "NSX A", "Ghi chú");

        // Assert
        Assert.Single(pickingNote.LineItems);
        var item = pickingNote.LineItems[0];
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
    }

    /// <summary>
    /// Kiểm tra cập nhật số lượng khi thêm sản phẩm đã tồn tại trong danh sách.
    /// </summary>
    [Fact]
    public void AddItem_ShouldUpdateQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var deliveryInfo = new DeliveryInfo("Receiver", "000", "Address", "Company", "Tax");
        var pickingNote = PickingNote.CreateForSalesOrder(Guid.NewGuid(), "PN-001", "Desc", deliveryInfo);
        var productId = ProductId.New();
        pickingNote.AddItem(productId, "P001", "Sản phẩm 1", "Cái", new Quantity(10), "NSX A", "Note 1");

        // Act
        pickingNote.AddItem(productId, "P001", "Sản phẩm 1", "Cái", new Quantity(5), "NSX A", "Note 2");

        // Assert
        Assert.Single(pickingNote.LineItems);
        Assert.Equal(new Quantity(15), pickingNote.LineItems[0].Quantity);
    }

    /// <summary>
    /// Kiểm tra cập nhật trạng thái khi bắt đầu soạn hàng (Picking).
    /// </summary>
    [Fact]
    public void StartPicking_ShouldUpdateStatus()
    {
        // Arrange
        var deliveryInfo = new DeliveryInfo("Receiver", "000", "Address", "Company", "Tax");
        var pickingNote = PickingNote.CreateForSalesOrder(Guid.NewGuid(), "PN-001", "Desc", deliveryInfo);

        // Act
        pickingNote.StartPicking();

        // Assert
        Assert.Equal(PickingStatus.Picking, pickingNote.Status);
    }

    /// <summary>
    /// Kiểm tra cập nhật trạng thái khi xác nhận hủy (Canceled).
    /// </summary>
    [Fact]
    public void ConfirmCanceled_ShouldUpdateStatus()
    {
        // Arrange
        var deliveryInfo = new DeliveryInfo("Receiver", "000", "Address", "Company", "Tax");
        var pickingNote = PickingNote.CreateForSalesOrder(Guid.NewGuid(), "PN-001", "Desc", deliveryInfo);

        // Act
        pickingNote.ConfirmCanceled();

        // Assert
        Assert.Equal(PickingStatus.Canceled, pickingNote.Status);
    }

    /// <summary>
    /// Kiểm tra cập nhật trạng thái khi hoàn tất soạn hàng (Completed).
    /// </summary>
    [Fact]
    public void ConfirmCompleted_ShouldUpdateStatus()
    {
        // Arrange
        var deliveryInfo = new DeliveryInfo("Receiver", "000", "Address", "Company", "Tax");
        var pickingNote = PickingNote.CreateForSalesOrder(Guid.NewGuid(), "PN-001", "Desc", deliveryInfo);

        // Act
        pickingNote.ConfirmCompleted();

        // Assert
        Assert.Equal(PickingStatus.Completed, pickingNote.Status);
    }
}
