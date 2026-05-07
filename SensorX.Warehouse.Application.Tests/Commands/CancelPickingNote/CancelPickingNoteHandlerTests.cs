using Ardalis.Specification;
using FluentAssertions;
using Moq;
using SensorX.Warehouse.Application.Commands.CancelPickingNote;
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

namespace SensorX.Warehouse.Application.Tests.Commands.CancelPickingNote;

public class CancelPickingNoteHandlerTests
{
    private readonly Mock<IRepository<PickingNote>> _pickingNoteRepositoryMock;
    private readonly Mock<IRepository<InventoryItem>> _inventoryItemRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly InventoryService _inventoryService;
    private readonly CancelPickingNoteHandler _handler;

    public CancelPickingNoteHandlerTests()
    {
        _pickingNoteRepositoryMock = new Mock<IRepository<PickingNote>>();
        _inventoryItemRepositoryMock = new Mock<IRepository<InventoryItem>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _inventoryService = new InventoryService();
        _handler = new CancelPickingNoteHandler(
            _pickingNoteRepositoryMock.Object,
            _inventoryItemRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _inventoryService
        );
    }

    [Fact]
    public async Task Handle_Should_CancelPicking_And_ReleaseAllocations()
    {
        // Arrange
        var pickingNoteId = PickingNoteId.New();
        var productId = ProductId.New();
        var pickingNote = new PickingNote(
            pickingNoteId,
            Code.Create("PN001"),
            new DocumentReference(DocumentType.SalesOrder, OrderId.New(), Code.Create("SO001")),
            PickingStatus.Picking,
            "Test",
            new DeliveryInfo("R", "0", "A", "C", "T")
        );
        pickingNote.AddItem(productId, Code.Create("PRD"), "P", "PCS", new Quantity(5), "M", "N");

        var inventoryItem = new InventoryItem(
            InventoryItemId.New(),
            productId,
            null,
            new Quantity(10),
            new Quantity(5) // allocated 5
        );

        _pickingNoteRepositoryMock.Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<ISpecification<PickingNote>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pickingNote);

        _inventoryItemRepositoryMock.Setup(x => x.ListAsync(
                It.IsAny<ISpecification<InventoryItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InventoryItem> { inventoryItem });

        var command = new CancelPickingNoteCommand { PickingNoteId = pickingNoteId.Value };
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        pickingNote.Status.Should().Be(PickingStatus.Canceled);
        inventoryItem.AllocatedQuantity.Value.Should().Be(0); // released

        _pickingNoteRepositoryMock.Verify(x => x.Update(pickingNote, It.IsAny<CancellationToken>()), Times.Once);
        _inventoryItemRepositoryMock.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<InventoryItem>>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}