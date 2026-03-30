using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.Events;

public record StockInCreatedEvent(
    Guid StockInId,
    string TransferOrderCode
) : IDomainEvent;