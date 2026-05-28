using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.Events;

public record StockOutCreatedEvent(
    Guid StockOutId,
    SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.DocumentType SourceType,
    Guid SourceId
) : IDomainEvent;
