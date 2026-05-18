using MediatR;
using MassTransit;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Application.Events;
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
    InventoryService _inventoryService,
    IPublishEndpoint _publishEndpoint
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
        var inventorySpec = new GetInventoryItemByProductIds(pickingNote.WarehouseId, [.. productIds]);
        var inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

        // 4. Cancel picking (release allocations)
        _inventoryService.CancelPicking(inventoryItems, pickingNote);

        // 5. Persist updates
        await _pickingNoteRepository.Update(pickingNote, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);

        var snapshotItems = pickingNote.LineItems.Select(line =>
        {
            var inventoryItem = inventoryItems.First(x => x.ProductId == line.ProductId);
            return new InventoryItemSnapshot(
                line.ProductId.Value,
                line.ProductCode.Value,
                line.ProductName,
                line.Unit,
                inventoryItem.PhysicalQuantity.Value,
                inventoryItem.AllocatedQuantity.Value,
                inventoryItem.WarehouseItemLocation?.WarehouseName,
                inventoryItem.WarehouseItemLocation?.BrandZone,
                inventoryItem.WarehouseItemLocation?.RackCode
            );
        }).ToList();

        await _publishEndpoint.Publish(new InventorySnapshotEvent(pickingNote.WarehouseId.Value.ToString(), DateTimeOffset.UtcNow, snapshotItems), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(pickingNote.Id.Value);
    }
}