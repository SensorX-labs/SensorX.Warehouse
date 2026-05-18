using MassTransit;
using SensorX.Warehouse.Application.Events.Consumers;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("Product-Status-Changed-Event")]
[EntityName("Product-Status-Changed-Event")]
public sealed record ChangeProductStatusEvent(
    Guid Id,
    ProductStatus Status,
    DateTimeOffset? UpdatedAt
);
