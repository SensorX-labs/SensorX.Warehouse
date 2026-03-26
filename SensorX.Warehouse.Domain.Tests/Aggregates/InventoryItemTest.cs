using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.Aggregates;

public class InventoryItemTests
{
    /// <summary>
    /// Hàm hỗ trợ khởi tạo nhanh đối tượng InventoryItem để test.
    /// Sử dụng WarehouseId.Default đã fix cứng trong Domain.
    /// </summary>
    private static InventoryItem CreateDefaultItem(int physical, int allocated)
    {
        return new InventoryItem(
            InventoryItemId.New(),
            ProductId.New(),
            WarehouseId.Default,
            new WarehouseItemLocation(WarehouseId.Default, "Kho Hải Phòng", "1", "A", "R01"),
            new Quantity(physical),
            new Quantity(allocated)
        );
    }

    // ============================================================
    // NGHIỆP VỤ GIỮ HÀNG (ALLOCATE)
    // ============================================================

    /// <summary>
    /// Kiểm tra giữ hàng thành công: Tăng AllocatedQuantity khi hàng khả dụng còn đủ.
    /// </summary>
    [Fact]
    public void Allocate_ShouldIncreaseAllocatedQuantity_WhenStockIsAvailable()
    {
        var item = CreateDefaultItem(10, 2); // Tồn vật lý 10, đã giữ 2 -> Có thể bán 8

        item.Allocate(new Quantity(5));

        Assert.Equal(7, (int)item.AllocatedQuantity); // 2 + 5 = 7
    }

    /// <summary>
    /// Kiểm tra chặn giữ hàng: Ném DomainException khi lượng hàng giữ vượt quá lượng có thể bán.
    /// </summary>
    [Fact]
    public void Allocate_ShouldThrowDomainException_WhenNotEnoughSalableStock()
    {
        var item = CreateDefaultItem(10, 8); // Tồn vật lý 10, đã giữ 8 -> Chỉ còn 2 để bán

        // Cố tình giữ 5 (vượt quá 2) -> Phải báo lỗi
        Assert.Throws<DomainException>(() => item.Allocate(new Quantity(5)));
    }

    // ============================================================
    // NGHIỆP VỤ HỦY GIỮ HÀNG (CANCEL ALLOCATION)
    // ============================================================

    /// <summary>
    /// Kiểm tra hủy giữ hàng: Giảm số lượng AllocatedQuantity để giải phóng hàng.
    /// </summary>
    [Fact]
    public void CancelAllocation_ShouldDecreaseAllocatedQuantity_WhenAmountIsVald()
    {
        var item = CreateDefaultItem(10, 5); // Đang giữ 5

        item.CancelAllocation(new Quantity(3));

        Assert.Equal(2, (int)item.AllocatedQuantity); // 5 - 3 = 2
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ khi hủy: Không cho phép hủy nhiều hơn số lượng đang được giữ.
    /// </summary>
    [Fact]
    public void CancelAllocation_ShouldThrowDomainException_WhenAmountExceedsAllocated()
    {
        var item = CreateDefaultItem(10, 2); // Đang giữ 2

        // Cố tình hủy giữ 5 (nhiều hơn 2) -> Phải báo lỗi
        Assert.Throws<DomainException>(() => item.CancelAllocation(new Quantity(5)));
    }

    // ============================================================
    // NGHIỆP VỤ XUẤT/NHẬP KHO (STOCK IN/OUT)
    // ============================================================

    /// <summary>
    /// Kiểm tra xuất kho: Giảm tồn kho vật lý (PhysicalQuantity) khi xác nhận xuất hàng.
    /// </summary>
    [Fact]
    public void ConfirmStockOut_ShouldDecreasePhysicalQuantity_WhenStockIsAvailable()
    {
        var item = CreateDefaultItem(10, 0);

        item.ConfirmStockOut(new Quantity(4));

        Assert.Equal(6, (int)item.PhysicalQuantity); // 10 - 4 = 6
    }

    /// <summary>
    /// Kiểm tra chặn xuất kho: Không cho phép xuất nhiều hơn số lượng thực tế đang có trong kho.
    /// </summary>
    [Fact]
    public void ConfirmStockOut_ShouldThrowDomainException_WhenPhysicalStockIsInsufficient()
    {
        var item = CreateDefaultItem(5, 0);

        // Cố tình xuất 10 (trong khi chỉ có 5) -> Phải báo lỗi
        Assert.Throws<DomainException>(() => item.ConfirmStockOut(new Quantity(10)));
    }

    /// <summary>
    /// Kiểm tra nhập kho: Tăng tồn kho vật lý khi xác nhận nhập thêm hàng.
    /// </summary>
    [Fact]
    public void ConfirmStockIn_ShouldIncreasePhysicalQuantity()
    {
        var item = CreateDefaultItem(10, 0);

        item.ConfirmStockIn(new Quantity(5));

        Assert.Equal(15, (int)item.PhysicalQuantity); // 10 + 5 = 15
    }

    // ============================================================
    // TÍNH TOÁN LOGIC
    // ============================================================

    /// <summary>
    /// Kiểm tra công thức tính tồn kho khả dụng: Salable = Physical - Allocated.
    /// </summary>
    [Fact]
    public void GetSalableQuantity_ShouldReturnCorrectAvailableStock()
    {
        var item = CreateDefaultItem(20, 5); // Có 20, giữ 5 -> Còn 15 để bán

        var result = item.GetSalableQuantity();

        Assert.Equal(15, result);
    }
}