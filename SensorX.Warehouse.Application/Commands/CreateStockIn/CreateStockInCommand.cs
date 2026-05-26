using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Application.Commands.CreateStockIn;

public class CreateStockInCommand : IRequest<Result<Guid>>
{
    public Guid WarehouseId { get; set; }
    public string? TransferOrderCode { get; set; }
    public required string DeliveredBy { get; set; }
    public required string WarehouseKeeper { get; set; }
    public string? Description { get; set; }
    public List<StockInItemCommand> Items { get; set; } = [];
}

public class StockInItemCommand
{
    public Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string ProductCode { get; set; }
    public required string Unit { get; set; }
    public int Quantity { get; set; }
    public string? Floor { get; set; }
    public string? BrandZone { get; set; }
    public string? RackCode { get; set; }
}