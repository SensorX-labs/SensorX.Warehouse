using Ardalis.Specification;
using FluentAssertions;
using MediatR;
using Moq;
using SensorX.Warehouse.Application.Commands.CreatePickingNote;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Application.Tests.Commands.CreatePickingNote;

public class CreatePickingNoteHandlerTests
{
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IRepository<PickingNote>> _pickingNoteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InventoryService _inventoryService;
    private readonly CreatePickingNoteHandler _handler;

    public CreatePickingNoteHandlerTests()
    {
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _pickingNoteRepositoryMock = new Mock<IRepository<PickingNote>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inventoryService = new InventoryService();

        _handler = new CreatePickingNoteHandler(
            _inventoryItemRepositoryMock.Object,
            _pickingNoteRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _inventoryService
        );
    }

    [Fact]
    public async Task Handle_Should_CreatePickingNoteForSalesOrder_WhenValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreatePickingNoteCommand
        {
            DocumentType = "SalesOrder",
            DocumentId = Guid.NewGuid(),
            Description = "Test picking note for sales order",
            DeliveryInfo = new DeliveryInfoDto
            {
                ReceiverName = "John Doe",
                ReceiverPhone = "1234567890",
                DeliveryAddress = "123 Street, City",
                CompanyName = "Acme Corp",
                TaxCode = "123456789"
            },
            Items = new List<PickingNoteItemDto>
            {
                new PickingNoteItemDto
                {
                    ProductId = productId,
                    ProductCode = "PRD001",
                    ProductName = "Product 1",
                    Unit = "PCS",
                    Quantity = 10,
                    ManufactureName = "Manufacturer A",
                    Note = "Test note"
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

        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { inventoryItem });

        PickingNote? capturedPickingNote = null;
        _pickingNoteRepositoryMock.Setup(x => x.Add(It.IsAny<PickingNote>(), It.IsAny<CancellationToken>()))
            .Callback<PickingNote, CancellationToken>((p, _) => capturedPickingNote = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _pickingNoteRepositoryMock.Verify(x => x.Add(It.IsAny<PickingNote>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedPickingNote.Should().NotBeNull();
        capturedPickingNote!.Id.Value.Should().Be(result.Value);
        capturedPickingNote.Code.Value.Should().StartWith("PN-");
        capturedPickingNote.Description.Should().Be(command.Description);
        capturedPickingNote.Status.Should().Be(PickingStatus.Pending);
        capturedPickingnote.LineItems.Should().HaveCount(1);
        var item = capturedPickingNote.LineItems[0];
        item.ProductId.Should().Be(new ProductId(productId));
        item.ProductCode.Value.Should().Be("PRD001");
        item.Quantity.Value.Should().Be(10);
    }

    [Fact]
    public async Task Handle_Should_CreatePickingNoteForTransferOrder_WhenValid()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreatePickingNoteCommand
        {
            DocumentType = "TransferOrder",
            DocumentId = Guid.NewGuid(),
            DocumentCode = "TO123",
            Description = "Test picking note for transfer order",
            DeliveryInfo = new DeliveryInfoDto
            {
                ReceiverName = "Jane Doe",
                ReceiverPhone = "0987654321",
                DeliveryAddress = "456 Avenue, Town",
                CompanyName = "Globex Corp",
                TaxCode = "987654321"
            },
            Items = new List<PickingNoteItemDto>
            {
                new PickingNoteItemDto
                {
                    ProductId = productId,
                    ProductCode = "PRD002",
                    ProductName = "Product 2",
                    Unit = "KG",
                    Quantity = 5,
                    ManufactureName = "Manufacturer B",
                    Note = "Another note"
                }
            }
        };

        var inventoryItem = new InventoryItem(
            InventoryItemId.New(),
            new ProductId(productId),
            null,
            new Quantity(50),
            new Quantity(0)
        );

        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { inventoryItem });

        PickingNote? capturedPickingNote = null;
        _pickingNoteRepositoryMock.Setup(x => x.Add(It.IsAny<PickingNote>(), It.IsAny<CancellationToken>()))
            .Callback<PickingNote, CancellationToken>((p, _) => capturedPickingNote = p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _pickingNoteRepositoryMock.Verify(x => x.Add(It.IsAny<PickingNote>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedPickingNote.Should().NotBeNull();
        capturedPickingNote!.Id.Value.Should().Be(result.Value);
        capturedPickingNote.Code.Value.Should().Be("TO123");
        capturedPickingNote.Description.Should().Be(command.Description);
        capturedPickingNote.Status.Should().Be(PickingStatus.Pending);
        capturedPickingNote.LineItems.Should().HaveCount(1);
        var item = capturedPickingNote.LineItems[0];
        item.ProductId.Should().Be(new ProductId(productId));
        item.ProductCode.Value.Should().Be("PRD002");
        item.Quantity.Value.Should().Be(5);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenInventoryItemNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreatePickingNoteCommand
        {
            DocumentType = "SalesOrder",
            DocumentId = Guid.NewGuid(),
            Description = "Test picking note",
            DeliveryInfo = new DeliveryInfoDto
            {
                ReceiverName = "John Doe",
                ReceiverPhone = "1234567890",
                DeliveryAddress = "123 Street, City",
                CompanyName = "Acme Corp",
                TaxCode = "123456789"
            },
            Items = new List<PickingNoteItemDto>
            {
                new PickingNoteItemDto
                {
                    ProductId = productId,
                    ProductCode = "PRD001",
                    ProductName = "Product 1",
                    Unit = "PCS",
                    Quantity = 10,
                    ManufactureName = "Manufacturer A",
                    Note = "Test note"
                }
            }
        };

        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Inventory item not found");

        _pickingNoteRepositoryMock.Verify(x => x.Add(It.IsAny<PickingNote>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _inventoryItemRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}