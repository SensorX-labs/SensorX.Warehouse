using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.StockOuts;

public record GetStockOutByIdQuery : IRequest<Result<StockOutDetailDto>>
{
    public Guid Id { get; init; }
    public Guid WarehouseId { get; init; }
}

public record StockOutDetailDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Reason => Description ?? "Kiểm kê định kỳ";
    public string Status => "Approved";
    public Guid? PickingNoteId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public List<StockOutItemDto> Items { get; init; } = [];
}

public record StockOutItemDto
{
    public Guid ProductId { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public double Quantity { get; init; }
    public double AdjustedQuantity { get; init; }
    public string? Note { get; init; }
}
