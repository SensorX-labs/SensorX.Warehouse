using MediatR;

namespace SensorX.Warehouse.Application.Commands.CreateStockIn;

public class CreateStockInCommand : IRequest<Guid>
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime StockInDate { get; set; }
    public string? Note { get; set; }
}