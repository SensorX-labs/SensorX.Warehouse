using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.CreateStockOut;

public class CreateStockOutCommand : IRequest<Result<Guid>>
{
    public Guid WarehouseId { get; set; }
    /// <summary>
    /// ID của phiếu soạn hàng đã hoàn thành. Nếu null thì là phiếu xuất điều chỉnh.
    /// </summary>
    public Guid? PickingNoteId { get; set; }
    public string? Description { get; set; }
    public List<StockOutItemDto>? Items { get; set; }
}

public class StockOutItemDto
{
    public Guid ProductId { get; set; }
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public required string Unit { get; set; }
    public int Quantity { get; set; }
    public required string ManufactureName { get; set; }
    public string? Note { get; set; }
}
