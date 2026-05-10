using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Commands.StartPickingNote;

public class StartPickingNoteHandler(
    IRepository<PickingNote> _pickingNoteRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService
) : IRequestHandler<StartPickingNoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(StartPickingNoteCommand request, CancellationToken cancellationToken)
    {
        // 1. Get PickingNote by ID
        var spec = new GetPickingNoteById(new PickingNoteId(request.PickingNoteId));
        var pickingNote = await _pickingNoteRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (pickingNote is null)
            return Result<Guid>.Failure("Picking note not found");

        // 2. Validate picking note is pending (can start only from Pending)
        if (pickingNote.Status != PickingStatus.Pending)
            return Result<Guid>.Failure($"Cannot start picking. Current status: {pickingNote.Status}");

        // 3. Load all inventory items referenced in the picking note
        var productIds = pickingNote.LineItems.Select(x => x.ProductId).Distinct().ToList();
        var inventorySpec = new GetInventoryItemByProductIds([.. productIds]);
        var inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

        // 4. Start picking via service (allocates inventory, updates status)
        _inventoryService.StartPicking(inventoryItems, pickingNote);

        // 5. Persist changes
        await _pickingNoteRepository.Update(pickingNote, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(pickingNote.Id.Value);
    }
}