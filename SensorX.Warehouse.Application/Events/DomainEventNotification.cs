using MediatR;
using SensorX.Warehouse.Application;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Application.Events;

public record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification where TDomainEvent : IDomainEvent;

