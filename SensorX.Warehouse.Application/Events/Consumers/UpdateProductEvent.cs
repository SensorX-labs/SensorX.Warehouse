using MassTransit;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("Product-Updated-Event")]
[EntityName("Product-Updated-Event")]
public sealed record UpdateProductEvent(
    Guid Id,
    string Name,
    string Manufacture,
    string Unit,
    DateTimeOffset? UpdatedAt
);
