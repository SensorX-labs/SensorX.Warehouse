using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.CreateStockAdjustment;

public class CreateStockAdjustmentCommand : IRequest<Result<Guid>>
{
    public required string Code { get; set; }
    public required string Reason { get; set; }
    public string? Description { get; set; }
    public List<StockAdjustmentItemDto> Items { get; set; } = [];
}

public class StockAdjustmentItemDto
{
    public Guid ProductId { get; set; }
    public required string ProductCode { get; set; }
    public required string ProductName { get; set; }
    public required string Unit { get; set; }
    public int AdjustedQuantity { get; set; }
    public string? Note { get; set; }
}