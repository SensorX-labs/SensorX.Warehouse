using MassTransit;

namespace SensorX.Warehouse.Application.Events.StockInCreated;

[MessageUrn("stock-in-created")]
public interface IStockInCreatedEvent
{
    Guid StockInId { get; set; }
    string TransferOrderCode { get; set; }
}