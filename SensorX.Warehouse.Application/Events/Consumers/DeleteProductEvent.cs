using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("Product-Deleted-Event")]
[EntityName("Product-Deleted-Event")]
public sealed record DeleteProductEvent(Guid Id);
