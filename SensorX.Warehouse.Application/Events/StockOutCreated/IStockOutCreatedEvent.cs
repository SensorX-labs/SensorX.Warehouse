using MassTransit;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Application.Events.StockOutCreated;

[MessageUrn("stock-out-created")]
[EntityName("stock-out-created")]
public interface IStockOutCreatedEvent
{
    Guid StockOutId { get; set; }
    SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.DocumentType SourceType { get; set; }
    Guid SourceId { get; set; }
}
