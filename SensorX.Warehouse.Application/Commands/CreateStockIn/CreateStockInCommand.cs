using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Application.Commands.CreateStockIn;

public class CreateStockInCommand : IRequest<Result<Guid>>
{
    public string? TransferOrderCode { get; set; }
    public required string DevliveredBy { get; set; }
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
}