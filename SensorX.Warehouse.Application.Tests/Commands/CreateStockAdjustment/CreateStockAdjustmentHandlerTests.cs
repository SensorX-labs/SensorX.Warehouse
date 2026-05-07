using Ardalis.Specification;
using FluentAssertions;
using Moq;
using SensorX.Warehouse.Application.Commands.CreateStockAdjustment;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockAdjustmentAggregate;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Application.Tests.Commands.CreateStockAdjustment;

public class CreateStockAdjustmentHandlerTests
{
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IRepository<StockAdjustment>> _adjustmentRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateStockAdjustmentHandler _handler;

    public CreateStockAdjustmentHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _adjustmentRepositoryMock = new Mock<IRepository<StockAdjustment>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateStockAdjustmentHandler(
            _adjustmentRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateStockAdjustment_And_ReturnId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreateStockAdjustmentCommand
        {
            Code = "ADJ001",
            Reason = "Test adjustment",
            Description = "Test description",
            Items = new List<StockAdjustmentItemDto>
            {
                new StockAdjustmentItemDto
                {
                    ProductId = productId,
                    ProductCode = "PRD001",
                    ProductName = "Product 1",
                    Unit = "PCS",
                    AdjustedQuantity = 10,
                    Note = "Increase stock"
                }
            }
        };

        StockAdjustment? capturedAdjustment = null;
        _adjustmentRepositoryMock.Setup(x => x.Add(It.IsAny<StockAdjustment>(), It.IsAny<CancellationToken>()))
            .Callback<StockAdjustment, CancellationToken>((adj, _) => capturedAdjustment = adj);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _adjustmentRepositoryMock.Verify(x => x.Add(It.IsAny<StockAdjustment>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        capturedAdjustment.Should().NotBeNull();
        capturedAdjustment!.Id.Value.Should().Be(result.Value);
        capturedAdjustment.Code.Value.Should().Be("ADJ001");
        capturedAdjustment.Reason.Should().Be("Test adjustment");
        capturedAdjustment.Items.Should().HaveCount(1);
        var item = capturedAdjustment.Items[0];
        item.ProductId.Value.Should().Be(productId);
        item.ProductCode.Value.Should().Be("PRD001");
        item.AdjustedQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Handle_Should_CombineItems_WhenSameProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreateStockAdjustmentCommand
        {
            Code = "ADJ001",
            Reason = "Test",
            Items = new List<StockAdjustmentItemDto>
            {
                new StockAdjustmentItemDto
                {
                    ProductId = productId,
                    ProductCode = "PRD001",
                    ProductName = "Product 1",
                    Unit = "PCS",
                    AdjustedQuantity = 10,
                    Note = "First"
                },
                new StockAdjustmentItemDto
                {
                    ProductId = productId,
                    ProductCode = "PRD001",
                    ProductName = "Product 1",
                    Unit = "PCS",
                    AdjustedQuantity = -3,
                    Note = "Second"
                }
            }
        };

        StockAdjustment? capturedAdjustment = null;
        _adjustmentRepositoryMock.Setup(x => x.Add(It.IsAny<StockAdjustment>(), It.IsAny<CancellationToken>()))
            .Callback<StockAdjustment, CancellationToken>((adj, _) => capturedAdjustment = adj);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedAdjustment.Should().NotBeNull();
        capturedAdjustment!.Items.Should().HaveCount(1);
        capturedAdjustment.Items[0].AdjustedQuantity.Should().Be(7);
    }
}