using Ardalis.Specification;
using FluentAssertions;
using MediatR;
using Moq;
using SensorX.Warehouse.Application.Commands.CreateStockOut;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.StockOutAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Application.Tests.Commands.CreateStockOut;

public class CreateStockOutHandlerTests
{
    private readonly Mock<IRepository<PickingNote>> _pickingNoteRepositoryMock;
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IRepository<StockOut>> _stockOutRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InventoryService _inventoryService;
    private readonly CreateStockOutHandler _handler;

    public CreateStockOutHandlerTests()
    {
        _pickingNoteRepositoryMock = new Mock<IRepository<PickingNote>>();
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _stockOutRepositoryMock = new Mock<IRepository<StockOut>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inventoryService = new InventoryService();

        _handler = new CreateStockOutHandler(
            _pickingNoteRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            _stockOutRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _inventoryService
        );
    }

    [Fact]
    public async Task Handle_Should_CreateStockOut_WhenPickingNoteCompleted()
    {
        // Arrange
        var pickingNoteId = PickingNoteId.New();
        var productId = ProductId.New();
        var pickingNote = new PickingNote(
            pickingNoteId,
            Code.Create("PN123"),
            new DocumentReference(DocumentType.SalesOrder, OrderId.New(), Code.Create("SO123")),
            PickingStatus.Completed,
            "Test description",
            new DeliveryInfo("Receiver", "0000000000", "Address", "Company", "TaxCode")
        );
        pickingNote.AddItem(productId, Code.Create("PRD"), "Product", "PCS", new Quantity(5), "Manufacturer", "Note");

        var inventoryItem = new InventoryItem(
            InventoryItemId.New(),
            productId,
            null,
            new Quantity(20),
            new Quantity(5) // allocated 5
        );

        // Setup repositories
        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pickingNote);

        var spec = new GetInventoryItemByProductIds([productId.Value]);
        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { inventoryItem });

        StockOut? capturedStockOut = null;
        _stockOutRepositoryMock.Setup(x => x.Add(It.IsAny<StockOut>(), It.IsAny<CancellationToken>()))
            .Callback<StockOut, CancellationToken>((s, _) => capturedStockOut = s);

        // Act
        var command = new CreateStockOutCommand { PickingNoteId = pickingNoteId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _pickingNoteRepositoryMock.Verify(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<PickingNote>>(), It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.ListAsync(It.IsAny<ISpecification<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        _stockOutRepositoryMock.Verify(x => x.Add(It.IsAny<StockOut>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedStockOut.Should().NotBeNull();
        capturedStockOut!.Id.Value.Should().Be(result.Value);
        capturedStockOut.PickingNoteId.Should().Be(pickingNoteId);
        capturedStockOut.LineItems.Should().HaveCount(1);
        var item = capturedStockOut.LineItems[0];
        item.ProductId.Should().Be(productId);
        item.ProductCode.Value.Should().Be("PRD");
        item.Quantity.Value.Should().Be(5);

        // Inventory updated: Physical -5, Allocated -5
        inventoryItem.PhysicalQuantity.Value.Should().Be(15);
        inventoryItem.AllocatedQuantity.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenPickingNoteNotFound()
    {
        // Arrange
        var pickingNoteId = PickingNoteId.New();

        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PickingNote?)null);

        // Act
        var command = new CreateStockOutCommand { PickingNoteId = pickingNoteId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Picking note not found");

        _pickingNoteRepositoryMock.Verify(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<PickingNote>>(), It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.ListAsync(It.IsAny<ISpecification<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        _stockOutRepositoryMock.Verify(x => x.Add(It.IsAny<StockOut>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenPickingNoteNotCompleted()
    {
        // Arrange
        var pickingNoteId = PickingNoteId.New();
        var pickingNote = new PickingNote(
            pickingNoteId,
            Code.Create("PN123"),
            new DocumentReference(DocumentType.SalesOrder, OrderId.New(), Code.Create("SO123")),
            PickingStatus.Pending, // Not completed
            "Test description",
            new DeliveryInfo("Receiver", "0000000000", "Address", "Company", "TaxCode")
        );

        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pickingNote);

        // Act
        var command = new CreateStockOutCommand { PickingNoteId = pickingNoteId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Picking note must be completed");

        _pickingNoteRepositoryMock.Verify(x => x.FirstOrDefaultAsync(It.IsAny<ISpecification<PickingNote>>(), It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.ListAsync(It.IsAny<ISpecification<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Never);
        _stockOutRepositoryMock.Verify(x => x.Add(It.IsAny<StockOut>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}