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

namespace SensorX.Warehouse.Application.Commands.CancelPickingNote;

public class CancelPickingNoteHandler(
    IRepository<PickingNote> _pickingNoteRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService
) : IRequestHandler<CancelPickingNoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CancelPickingNoteCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve picking note
        var spec = new GetPickingNoteById(new PickingNoteId(request.PickingNoteId));
        var pickingNote = await _pickingNoteRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (pickingNote is null)
            return Result<Guid>.Failure("Picking note not found");

        // 2. Validate can cancel (Pending or Picking)
        if (pickingNote.Status == PickingStatus.Canceled)
            return Result<Guid>.Failure("Picking note already canceled");

        // 3. Load related inventory items for allocation rollback
        var productIds = pickingNote.LineItems.Select(x => x.ProductId).Distinct().ToList();
        var inventorySpec = new GetInventoryItemByProductIds([.. productIds]);
        var inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

        // 4. Cancel picking (release allocations)
        _inventoryService.CancelPicking(inventoryItems, pickingNote);

        // 5. Persist updates
        await _pickingNoteRepository.Update(pickingNote, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(pickingNote.Id.Value);
    }
}