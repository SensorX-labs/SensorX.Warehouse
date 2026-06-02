using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

namespace SensorX.Warehouse.Application.Queries.GetPickingNote;

public class GetPickingNoteQuery : IRequest<Result<PickingNoteDto>>
{
    public Guid PickingNoteId { get; set; }
    public Guid WarehouseId { get; set; }
}

public record PickingNoteDto(
    Guid Id,
    Guid WarehouseId,
    string Code,
    string? Description,
    PickingStatus Status,
    PickingNoteDeliveryInfoDto DeliveryInfo,
    List<PickingNoteQueryItemDto> Items,
    string? TransferOrderCode = null,
    Guid? SourceDocumentId = null,
    int? SourceDocumentType = null
);

public record PickingNoteDeliveryInfoDto(
    string ReceiverName,
    string ReceiverPhone,
    string DeliveryAddress,
    string CompanyName,
    string TaxCode
);

public record PickingNoteQueryItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    string ManufactureName,
    string? Note
);
