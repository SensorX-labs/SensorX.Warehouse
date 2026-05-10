using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Queries.StockIns;

public class GetStockInByIdQuery : IRequest<Result<StockInDetailDto>>
{
    public Guid Id { get; set; }
}

public record StockInDetailDto(
    Guid Id,
    string Code,
    string? TransferOrderCode,
    string? Description,
    DateTimeOffset ReceivedDate,
    string CreatedBy,
    string DeliveredBy,
    string WarehouseKeeper,
    List<StockInItemDto> Items
);

public record StockInItemDto(
    Guid ProductId,
    string ProductName,
    string ProductCode,
    string Unit,
    int Quantity
);
