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

    private string? _description;
    public string? Description 
    { 
        get => string.IsNullOrEmpty(_description) ? Reason : _description; 
        set => _description = value; 
    }

    public string? Reason { get; set; }
    public string? Code { get; set; }
    public bool IsAdjustment { get; set; }
    public List<StockOutItemDto>? Items { get; set; }
}

public class StockOutItemDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = "Cái";

    private int _quantity;
    public int Quantity 
    { 
        get => _quantity != 0 ? _quantity : Math.Abs(AdjustedQuantity); 
        set => _quantity = value; 
    }

    public int AdjustedQuantity { get; set; }
    public string ManufactureName { get; set; } = string.Empty;
    public string? Note { get; set; }
}
