using Ardalis.Specification;
using FluentAssertions;
using Moq;
using SensorX.Warehouse.Application.Commands.CompletePickingNote;
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

namespace SensorX.Warehouse.Application.Tests.Commands.CompletePickingNote;

public class CompletePickingNoteHandlerTests
{
    private readonly Mock<IRepository<PickingNote>> _pickingNoteRepositoryMock;
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InventoryService _inventoryService;
    private readonly CompletePickingNoteHandler _handler;

    public CompletePickingNoteHandlerTests()
    {
        _pickingNoteRepositoryMock = new Mock<IRepository<PickingNote>>();
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inventoryService = new InventoryService();
        _handler = new CompletePickingNoteHandler(
            _pickingNoteRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _inventoryService
        );
    }

    [Fact]
    public async Task Handle_Should_CompletePicking_WhenPickingNoteIsInProgress()
    {
        // Arrange
        var pickingNoteId = PickingNoteId.New();
        var productId = ProductId.New();
        var pickingNote = CreatePickingPickingNote(pickingNoteId, productId);

        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pickingNote);

        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem>());

        var command = new CompletePickingNoteCommand { PickingNoteId = pickingNoteId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(pickingNoteId.Value);
        pickingNote.Status.Should().Be(PickingStatus.Completed);

        _pickingNoteRepositoryMock.Verify(x => x.Update(pickingNote, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenPickingNoteNotFound()
    {
        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PickingNote?)null);

        var command = new CompletePickingNoteCommand { PickingNoteId = Guid.NewGuid() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Picking note not found");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenPickingNoteNotInProgress()
    {
        var pickingNoteId = PickingNoteId.New();
        var pickingNote = new PickingNote(
            pickingNoteId,
            Code.Create("PN001"),
            new DocumentReference(DocumentType.SalesOrder, OrderId.New(), Code.Create("SO001")),
            PickingStatus.Pending, // Not in Picking status
            "Test",
            new DeliveryInfo("R", "0", "A", "C", "T")
        );

        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pickingNote);

        var command = new CompletePickingNoteCommand { PickingNoteId = pickingNoteId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot complete picking");
    }

    private static PickingNote CreatePickingPickingNote(PickingNoteId pickingNoteId, ProductId productId)
    {
        var pickingNote = new PickingNote(
            pickingNoteId,
            Code.Create("PN001"),
            new DocumentReference(DocumentType.SalesOrder, OrderId.New(), Code.Create("SO001")),
            PickingStatus.Picking, // Already in progress
            "Test picking note",
            new DeliveryInfo("Receiver", "1234567890", "Address", "Company", "TaxCode")
        );
        pickingNote.AddItem(productId, Code.Create("PRD"), "Product", "PCS", new Quantity(10), "Manufacturer", "Note");
        return pickingNote;
    }
}