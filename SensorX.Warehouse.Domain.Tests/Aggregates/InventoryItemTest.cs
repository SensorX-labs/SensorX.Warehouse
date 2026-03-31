using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.Common.Exceptions;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Domain.Tests.Aggregates;

public class InventoryItemTests
{
    /// <summary>
    /// Helper khởi tạo nhanh InventoryItem để test.
    /// </summary>
    private static InventoryItem CreateDefaultItem(int physical, int allocated)
    {
        return new InventoryItem(
            InventoryItemId.New(),
            ProductId.New(),
            new WarehouseItemLocation("Kho Hải Phòng", "1", "A", "R01"),
            new Quantity(physical),
            new Quantity(allocated)
        );
    }

    // --- NGHIỆP VỤ GIỮ HÀNG (ALLOCATE) ---

    /// <summary>
    /// Tăng AllocatedQuantity khi hàng khả dụng còn đủ.
    /// </summary>
    [Fact]
    public void Allocate_ShouldIncreaseAllocatedQuantity_WhenStockIsAvailable()
    {
        var item = CreateDefaultItem(10, 2);

        item.Allocate(new Quantity(5));

        Assert.Equal(7, (int)item.AllocatedQuantity);
    }

    /// <summary>
    /// Ném DomainException khi lượng hàng giữ vượt quá lượng khả dụng.
    /// </summary>
    [Fact]
    public void Allocate_ShouldThrowDomainException_WhenNotEnoughSalableStock()
    {
        var item = CreateDefaultItem(10, 8);

        Assert.Throws<DomainException>(() => item.Allocate(new Quantity(5)));
    }

    // --- NGHIỆP VỤ HỦY GIỮ HÀNG (CANCEL ALLOCATION) ---

    /// <summary>
    /// Giảm AllocatedQuantity để giải phóng hàng.
    /// </summary>
    [Fact]
    public void CancelAllocation_ShouldDecreaseAllocatedQuantity_WhenAmountIsVald()
    {
        var item = CreateDefaultItem(10, 5);

        item.CancelAllocation(new Quantity(3));

        Assert.Equal(2, (int)item.AllocatedQuantity);
    }

    /// <summary>
    /// Không cho phép hủy nhiều hơn số lượng đang được giữ.
    /// </summary>
    [Fact]
    public void CancelAllocation_ShouldThrowDomainException_WhenAmountExceedsAllocated()
    {
        var item = CreateDefaultItem(10, 2);

        Assert.Throws<DomainException>(() => item.CancelAllocation(new Quantity(5)));
    }

    // --- NGHIỆP VỤ XUẤT/NHẬP KHO (STOCK IN/OUT) ---

    /// <summary>
    /// Giảm tồn kho vật lý khi xác nhận xuất hàng.
    /// </summary>
    [Fact]
    public void ConfirmStockOut_ShouldDecreasePhysicalQuantity_WhenStockIsAvailable()
    {
        var item = CreateDefaultItem(10, 0);

        item.ConfirmStockOut(new Quantity(4));

        Assert.Equal(6, (int)item.PhysicalQuantity);
    }

    /// <summary>
    /// Không cho phép xuất nhiều hơn tồn kho thực tế.
    /// </summary>
    [Fact]
    public void ConfirmStockOut_ShouldThrowDomainException_WhenPhysicalStockIsInsufficient()
    {
        var item = CreateDefaultItem(5, 0);

        Assert.Throws<DomainException>(() => item.ConfirmStockOut(new Quantity(10)));
    }

    /// <summary>
    /// Tăng tồn kho vật lý khi xác nhận nhập hàng.
    /// </summary>
    [Fact]
    public void ConfirmStockIn_ShouldIncreasePhysicalQuantity()
    {
        var item = CreateDefaultItem(10, 0);

        item.ConfirmStockIn(new Quantity(5));

        Assert.Equal(15, (int)item.PhysicalQuantity);
    }

    // --- TÍNH TOÁN LOGIC ---

    /// <summary>
    /// Tồn kho khả dụng: Salable = Physical - Allocated.
    /// </summary>
    [Fact]
    public void GetSalableQuantity_ShouldReturnCorrectAvailableStock()
    {
        var item = CreateDefaultItem(20, 5);

        var result = item.GetSalableQuantity();

        Assert.Equal(15, result);
    }
}