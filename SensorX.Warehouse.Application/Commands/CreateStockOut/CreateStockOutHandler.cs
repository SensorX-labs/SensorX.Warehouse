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
        var warehouseId = new WarehouseId(request.WarehouseId);
        StockOut stockOut;
        List<InventoryItem> inventoryItems;

        if (request.PickingNoteId.HasValue)
        {
            // Case 1: Create StockOut from PickingNote
            var spec = new GetPickingNoteById(new PickingNoteId(request.PickingNoteId.Value));
            var pickingNote = await _pickingNoteRepository.FirstOrDefaultAsync(spec, cancellationToken);
            if (pickingNote is null)
                return Result<Guid>.Failure("Picking note not found");

            if (pickingNote.Status != PickingStatus.Completed)
                return Result<Guid>.Failure($"Picking note must be completed to create stock out. Current status: {pickingNote.Status}");

            var productIds = pickingNote.LineItems.Select(x => x.ProductId).Distinct().ToList();
            var inventorySpec = new GetInventoryItemByProductIds(warehouseId, [.. productIds]);
            inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

            stockOut = _inventoryService.CreateStockOutFromPickingNote(inventoryItems, pickingNote);
        }
        else
        {
            // Case 2: Direct adjustment
            if (request.Items == null || request.Items.Count == 0)
                return Result<Guid>.Failure("Items must be provided for direct adjustment.");

            var productIds = request.Items.Select(x => new ProductId(x.ProductId)).Distinct().ToList();
            var inventorySpec = new GetInventoryItemByProductIds(warehouseId, productIds);
            inventoryItems = await _inventoryItemRepository.ListAsync(inventorySpec, cancellationToken);

            // For simplicity, we just handle one item for now or multiple items? 
            // The AdjustInventory in service currently handles ONE item.
            // Let's create the StockOut directly and update items.
            var isAdjustment = request.IsAdjustment || !request.PickingNoteId.HasValue;
            var prefix = isAdjustment ? "PKK" : "PX";
            var code = !string.IsNullOrEmpty(request.Code) ? Code.From(request.Code) : Code.Create(prefix);
            stockOut = new StockOut(StockOutId.New(), warehouseId, code, request.Description, null);
            foreach (var itemDto in request.Items)
            {
                var inventoryItem = inventoryItems.FirstOrDefault(x => x.ProductId == itemDto.ProductId);
                if (inventoryItem == null) return Result<Guid>.Failure($"Product {itemDto.ProductId} not found in inventory.");
                var encodedNote = isAdjustment ? $"[Adj:{itemDto.AdjustedQuantity}] {itemDto.Note}".Trim() : itemDto.Note;

                stockOut.AddItem(
                    new ProductId(itemDto.ProductId),
                    Code.From(itemDto.ProductCode),
                    itemDto.ProductName,
                    itemDto.Unit,
                    new Quantity(itemDto.Quantity),
                    itemDto.ManufactureName,
                    encodedNote
                );
                
                var delta = itemDto.AdjustedQuantity != 0 ? itemDto.AdjustedQuantity : -itemDto.Quantity;
                inventoryItem.AdjustPhysicalQuantity(delta);
            }
        }

        await _stockOutRepository.Add(stockOut, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(stockOut.Id.Value);
    }
}
