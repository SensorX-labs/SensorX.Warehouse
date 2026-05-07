using Ardalis.Specification;
using FluentAssertions;
using Moq;
using SensorX.Warehouse.Application.Commands.ApproveStockAdjustment;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;
using Xunit;

namespace SensorX.Warehouse.Application.Tests.Commands.ApproveStockAdjustment;

public class ApproveStockAdjustmentHandlerTests
{
    private readonly Mock<IRepository<StockAdjustment>> _adjustmentRepositoryMock;
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InventoryService _inventoryService;
    private readonly ApproveStockAdjustmentHandler _handler;

    public ApproveStockAdjustmentHandlerTests()
    {
        _adjustmentRepositoryMock = new Mock<IRepository<StockAdjustment>>();
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inventoryService = new InventoryService();
        _handler = new ApproveStockAdjustmentHandler(
            _adjustmentRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _inventoryService
        );
    }

    [Fact]
    public async Task Handle_Should_ApproveAdjustment_And_ApplyToInventory()
    {
        // Arrange
        var adjustmentId = StockAdjustmentId.New();
        var productId = ProductId.New();
        var adjustment = new StockAdjustment(
            adjustmentId,
            Code.From("ADJ001"),
            "Reason",
            "Description"
        );
        adjustment.AddItem(productId, Code.From("PRD"), "Product", "PCS", 10, "Note");

        var inventoryItem = new InventoryItem(
            InventoryItemId.New(),
            productId,
            null,
            new Quantity(100),
            new Quantity(0)
        );

        _adjustmentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<StockAdjustment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(adjustment);

        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { inventoryItem });

        // Act
        var command = new ApproveStockAdjustmentCommand { Id = adjustmentId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        adjustment.Status.Should().Be(AdjustmentStatus.Approved);
        inventoryItem.PhysicalQuantity.Value.Should().Be(110); // 100 + 10

        _adjustmentRepositoryMock.Verify(x => x.Update(adjustment, It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenAdjustmentNotFound()
    {
        _adjustmentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<StockAdjustment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockAdjustment?)null);

        var command = new ApproveStockAdjustmentCommand { Id = Guid.NewGuid() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Adjustment not found");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenNotPending()
    {
        var adjustment = new StockAdjustment(
            StockAdjustmentId.New(),
            Code.From("ADJ001"),
            "Reason",
            "Description"
        );
        adjustment.Approve(); // already approved

        _adjustmentRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<StockAdjustment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(adjustment);

        var command = new ApproveStockAdjustmentCommand { Id = adjustment.Id.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot approve adjustment in status");
    }
}
