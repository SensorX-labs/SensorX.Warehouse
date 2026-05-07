using MediatR;
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

namespace SensorX.Warehouse.Application.Commands.CreateStockOut;

public class CreateStockOutHandler(
    IRepository<PickingNote> _pickingNoteRepository,
    IRepository<InventoryItem> _inventoryItemRepository,
    IRepository<StockOut> _stockOutRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService
) : IRequestHandler<CreateStockOutCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateStockOutCommand request, CancellationToken cancellationToken)
    {
        // 1. Get PickingNote by ID
        var spec = new GetPickingNoteById(new PickingNoteId(request.PickingNoteId));
        var pickingNote = await _pickingNoteRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (pickingNote == null)
            return Result<Guid>.Failure("Picking note not found");

        // 2. Validate picking note is completed
        if (pickingNote.Status != PickingStatus.Completed)
            return Result<Guid>.Failure($"Picking note must be completed to create stock out. Current status: {pickingNote.Status}");

        // 3. Load all inventory items referenced in the picking note
        var productIds = pickingNote.LineItems.Select(x => x.ProductId).Distinct().ToList();
        var inventorySpec = new GetInventoryItemByProductIds([.. productIds]);
        var inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

        // 4. Create StockOut from PickingNote using domain service
        var stockOut = _inventoryService.CreateStockOutFromPickingNote(inventoryItems, pickingNote);

        // 5. Persist changes
        await _stockOutRepository.Add(stockOut, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(stockOut.Id.Value);
    }
}
