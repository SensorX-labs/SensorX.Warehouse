using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Commands.CreatePickingNote;

public class CreatePickingNoteCommand : IRequest<Result<Guid>>
{
    /// <summary>
    /// Loại document nguồn: SalesOrder hoặc TransferOrder.
    /// </summary>
    public required string DocumentType { get; set; }

    /// <summary>
    /// ID của document nguồn (OrderId hoặc TransferOrderId).
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Mã document nguồn (tùy chọn).
    /// </summary>
    public string? DocumentCode { get; set; }

    /// <summary>
    /// Mô tả phiếu nhặt hàng.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Thông tin giao hàng.
    /// </summary>
    public required DeliveryInfoDto DeliveryInfo { get; set; }

    /// <summary>
    /// Danh sách sản phẩm cần nhặt.
    /// </summary>
    public List<PickingNoteItemDto> Items { get; set; } = [];
}

public class DeliveryInfoDto
{
    public required string ReceiverName { get; set; }
    public required string ReceiverPhone { get; set; }
    public required string DeliveryAddress { get; set; }
    public required string CompanyName { get; set; }
    public required string TaxCode { get; set; }

    public DeliveryInfo ToDeliveryInfo() => new(
        ReceiverName,
        ReceiverPhone,
        DeliveryAddress,
        CompanyName,
        TaxCode
    );
}

public class PickingNoteItemDto
{
    public Guid ProductId { get; set; }
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public required string Unit { get; set; }
    public int Quantity { get; set; }
    public required string ManufactureName { get; set; }
    public string? Note { get; set; }
}
