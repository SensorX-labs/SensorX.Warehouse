using MassTransit;
using MediatR;
using SensorX.Warehouse.Domain.Events;

namespace SensorX.Warehouse.Application.Events.StockInCreated;

public class StockInCreatedEventHandler(IPublishEndpoint _publishEndpoint)
    : INotificationHandler<DomainEventNotification<StockInCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<StockInCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        await _publishEndpoint.Publish<IStockInCreatedEvent>(domainEvent, cancellationToken);
    }
}