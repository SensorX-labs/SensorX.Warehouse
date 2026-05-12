using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate.Specifications;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.Services;
using SensorX.Warehouse.Domain.StrongIDs;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Commands.CreatePickingNote;

public class CreatePickingNoteHandler(
    IRepository<InventoryItem> _inventoryItemRepository,
    IRepository<PickingNote> _pickingNoteRepository,
    IUnitOfWork _unitOfWork,
    InventoryService _inventoryService
) : IRequestHandler<CreatePickingNoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePickingNoteCommand request, CancellationToken cancellationToken)
    {
        var warehouseId = new WarehouseId(request.WarehouseId);
        // 1. Build source document
        var docType = Enum.Parse<DocumentType>(request.DocumentType, ignoreCase: true);
        Code? noteCode = null;
        
        // 2. Build delivery info
        var deliveryInfo = request.DeliveryInfo.ToDeliveryInfo();

        // 3. Create PickingNote aggregate
        PickingNote pickingNote;
        if (docType == DocumentType.SalesOrder)
        {
            var orderId = new OrderId(request.DocumentId);
            noteCode = Code.Create("PN");
            pickingNote = PickingNote.CreateForSalesOrder(warehouseId, orderId, noteCode, request.Description, deliveryInfo);
        }
        else
        {
            var transferOrderId = new TransferOrderId(request.DocumentId);
            noteCode = Code.Create("TN");
            pickingNote = PickingNote.CreateForTransferOrder(warehouseId, transferOrderId, noteCode, request.Description, deliveryInfo);
        }

        // 4. Load inventory items to allocate
        var productIds = request.Items.Select(x => x.ProductId).ToList();
        var spec = new GetInventoryItemByProductIds(warehouseId, [.. productIds]);
        var inventoryItems = await _inventoryItemRepository.ListAsync(spec, cancellationToken);

        // 5. Add items to picking note
        foreach (var itemDto in request.Items)
        {
            var productId = new ProductId(itemDto.ProductId);
            var productCode = Code.Create("P");
            pickingNote.AddItem(
                productId,
                productCode,
                itemDto.ProductName,
                itemDto.Unit,
                new Quantity(itemDto.Quantity),
                itemDto.ManufactureName,
                itemDto.Note ?? string.Empty
            );
        }

        // 6. Start picking → allocate inventory
        _inventoryService.StartPicking(inventoryItems, pickingNote);

        // 7. Persist
        await _pickingNoteRepository.Add(pickingNote, cancellationToken);
        await _inventoryItemRepository.UpdateRange(inventoryItems, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(pickingNote.Id.Value);
    }
}
