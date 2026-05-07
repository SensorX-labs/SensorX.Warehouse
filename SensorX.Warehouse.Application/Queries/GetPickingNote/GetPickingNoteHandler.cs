using MediatR;
using SensorX.Warehouse.Application.Common.Interfaces;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Application.Queries.GetPickingNote;

public class GetPickingNoteHandler(
    IRepository<PickingNote> _pickingNoteRepository
) : IRequestHandler<GetPickingNoteQuery, Result<PickingNoteDto>>
{
    public async Task<Result<PickingNoteDto>> Handle(GetPickingNoteQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetPickingNoteById(new PickingNoteId(request.PickingNoteId));
        var pickingNote = await _pickingNoteRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (pickingNote == null)
            return Result<PickingNoteDto>.Failure("Picking note not found");

        var dto = new PickingNoteDto(
            pickingNote.Id.Value,
            pickingNote.Code.Value,
            pickingNote.Description,
            pickingNote.Status,
            new PickingNoteDeliveryInfoDto(
                pickingNote.DeliveryInfo.ReceiverName,
                pickingNote.DeliveryInfo.ReceiverPhone,
                pickingNote.DeliveryInfo.DeliveryAddress,
                pickingNote.DeliveryInfo.CompanyName,
                pickingNote.DeliveryInfo.TaxCode
            ),
            pickingNote.LineItems.Select(item => new PickingNoteQueryItemDto(
                item.ProductId.Value,
                item.ProductCode.Value,
                item.ProductName,
                item.Unit,
                item.Quantity.Value,
                item.ManufactureName,
                item.Note
            )).ToList()
        );

        return Result<PickingNoteDto>.Success(dto);
    }
}