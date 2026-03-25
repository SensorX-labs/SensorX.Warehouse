using SensorX.Warehouse.Domain.SeedWork;
using MediatR;

namespace SensorX.Warehouse.Application.Common.DomainEvent;
public record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent): INotification where TDomainEvent : IDomainEvent;

