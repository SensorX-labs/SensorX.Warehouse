using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.GetStockAdjustment;

public class GetStockAdjustmentQuery : IRequest<Result<StockAdjustmentDto>>
{
    public Guid Id { get; set; }
}

public record StockAdjustmentDto(
    Guid Id,
    string Code,
    string Reason,
    string? Description,
    string Status,
    List<StockAdjustmentQueryItemDto> Items
);

public record StockAdjustmentQueryItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    int AdjustedQuantity,
    string? Note
);
