using Ardalis.Specification;
using FluentAssertions;
using Moq;
using SensorX.Warehouse.Application.Commands.StartPickingNote;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;
using Xunit;

namespace SensorX.Warehouse.Application.Tests.Commands.StartPickingNote;

public class StartPickingNoteHandlerTests
{
    private readonly Mock<IRepository<PickingNote>> _pickingNoteRepositoryMock;
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InventoryService _inventoryService;
    private readonly StartPickingNoteHandler _handler;

    public StartPickingNoteHandlerTests()
    {
        _pickingNoteRepositoryMock = new Mock<IRepository<PickingNote>>();
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inventoryService = new InventoryService();
        _handler = new StartPickingNoteHandler(
            _pickingNoteRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _inventoryService
        );
    }

    [Fact]
    public async Task Handle_Should_StartPicking_WhenPickingNoteIsPending()
    {
        // Arrange
        var pickingNoteId = PickingNoteId.New();
        var productId = ProductId.New();
        var inventoryItem = new InventoryItem(
            InventoryItemId.New(),
            productId,
            null,
            new Quantity(100),
            new Quantity(0)
        );

        var pickingNote = CreatePendingPickingNote(pickingNoteId, productId);

        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pickingNote);

        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { inventoryItem });

        // Act
        var command = new StartPickingNoteCommand { PickingNoteId = pickingNoteId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(pickingNoteId.Value);

        pickingNote.Status.Should().Be(PickingStatus.Picking);
        inventoryItem.AllocatedQuantity.Value.Should().Be(10); // allocated from line item

        _pickingNoteRepositoryMock.Verify(x => x.Update(pickingNote, It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenPickingNoteNotFound()
    {
        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PickingNote?)null);

        var command = new StartPickingNoteCommand { PickingNoteId = Guid.NewGuid() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Picking note not found");

        _inventoryItemRepositoryMock.Verify(x => x.ListAsync(It.IsAny<ISpecification<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenPickingNoteNotPending()
    {
        var pickingNote = CreatePendingPickingNote(PickingNoteId.New(), ProductId.New());
        pickingNote.StartPicking(); // already picking

        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pickingNote);

        var command = new StartPickingNoteCommand { PickingNoteId = pickingNote.Id.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot start picking");
    }

    private static PickingNote CreatePendingPickingNote(PickingNoteId pickingNoteId, ProductId productId)
    {
        var pickingNote = new PickingNote(
            pickingNoteId,
            Code.Create("PN001"),
            new DocumentReference(DocumentType.SalesOrder, OrderId.New(), Code.Create("SO001")),
            PickingStatus.Pending,
            "Test picking note",
            new DeliveryInfo("Receiver", "1234567890", "Address", "Company", "TaxCode")
        );
        pickingNote.AddItem(productId, Code.Create("PRD"), "Product", "PCS", new Quantity(10), "Manufacturer", "Note");
        return pickingNote;
    }
}