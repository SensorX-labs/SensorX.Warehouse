using MassTransit;
using MediatR;
using SensorX.Warehouse.Domain.Events;

namespace SensorX.Warehouse.Application.Events.StockOutCreated;

public class StockOutCreatedEventHandler(IPublishEndpoint _publishEndpoint)
    : INotificationHandler<DomainEventNotification<StockOutCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<StockOutCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        await _publishEndpoint.Publish<IStockOutCreatedEvent>(new
        {
            domainEvent.StockOutId,
            domainEvent.SourceType,
            domainEvent.SourceId
        }, cancellationToken);
    }
}
