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

namespace SensorX.Warehouse.Application.Commands.CompletePickingNote;

public class CompletePickingNoteHandler(
    IRepository<PickingNote> _pickingNoteRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService
) : IRequestHandler<CompletePickingNoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CompletePickingNoteCommand request, CancellationToken cancellationToken)
    {
        // 1. Get PickingNote by ID
        var spec = new GetPickingNoteById(new PickingNoteId(request.PickingNoteId));
        var pickingNote = await _pickingNoteRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (pickingNote == null)
            return Result<Guid>.Failure("Picking note not found");

        // 2. Validate picking note is currently in Picking status (can complete only from Picking)
        if (pickingNote.Status != PickingStatus.Picking)
            return Result<Guid>.Failure($"Cannot complete picking. Current status: {pickingNote.Status}");

        // 3. Load inventory items to verify allocations (optional, but good practice)
        var productIds = pickingNote.LineItems.Select(x => x.ProductId).Distinct().ToList();
        var inventorySpec = new GetInventoryItemByProductIds([.. productIds]);
        var inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

        // 4. Complete picking (no inventory change, just status update)
        pickingNote.ConfirmCompleted();

        // 5. Persist changes
        await _pickingNoteRepository.Update(pickingNote, cancellationToken);
        // Note: inventory allocations remain; they will be consumed when StockOut is created.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(pickingNote.Id.Value);
    }
}