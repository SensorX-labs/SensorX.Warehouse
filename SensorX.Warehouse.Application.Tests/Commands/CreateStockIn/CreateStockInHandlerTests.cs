using Ardalis.Specification;
using Moq;
using FluentAssertions;
using SensorX.Warehouse.Application.Commands.CreateStockIn;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.StockInAggregate;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;
using SensorX.Warehouse.Domain.Services.DTOs;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.Events;

namespace SensorX.Warehouse.Application.Tests.Commands.CreateStockIn;

public class CreateStockInHandlerTests
{
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IRepository<StockIn>> _stockInRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly InventoryService _inventoryService;
    private readonly CreateStockInHandler _handler;

    public CreateStockInHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _stockInRepositoryMock = new Mock<IRepository<StockIn>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserMock = new Mock<ICurrentUser>();
        _inventoryService = new InventoryService();

        _handler = new CreateStockInHandler(
            _inventoryItemRepositoryMock.Object,
            _stockInRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _inventoryService,
            _currentUserMock.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateStockIn_And_SaveToRepository()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreateStockInCommand
        {
            DevliveredBy = "Tung Sk",
            WarehouseKeeper = "Keeper A",
            Description = "Test Stock In",
            TransferOrderCode = Code.Create("TO").Value,
            Items = new List<StockInItemCommand>
            {
                new StockInItemCommand
                {
                    ProductId = productId,
                    ProductCode = Code.Create("PRD").Value,
                    ProductName = "Product 1",
                    Unit = "PCS",
                    Quantity = 10
                }
            }
        };

        var inventoryItem = new InventoryItem(
            InventoryItemId.New(),
            new ProductId(productId),
            null,
            new Quantity(100),
            new Quantity(0)
        );

        _currentUserMock.Setup(x => x.Username).Returns("testuser");
        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(It.IsAny<ISpecification<InventoryItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { inventoryItem });

        // Capture the stockIn object passed to repository
        StockIn? capturedStockIn = null;
        _stockInRepositoryMock.Setup(x => x.Add(It.IsAny<StockIn>(), It.IsAny<CancellationToken>()))
            .Callback<StockIn, CancellationToken>((s, _) => capturedStockIn = s);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _stockInRepositoryMock.Verify(x => x.Add(It.IsAny<StockIn>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Check Domain Event
        capturedStockIn.Should().NotBeNull();
        capturedStockIn!.DomainEvents.Should().ContainSingle(x => x is StockInCreatedEvent);
        var domainEvent = capturedStockIn.DomainEvents.OfType<StockInCreatedEvent>().Single();
        domainEvent.StockInId.Should().Be(result.Value);
        domainEvent.TransferOrderCode.Should().Be(command.TransferOrderCode);
    }
}
