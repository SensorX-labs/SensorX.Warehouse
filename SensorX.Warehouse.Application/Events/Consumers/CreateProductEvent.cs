using MassTransit;
using SensorX.Warehouse.Application.Events.Consumers;

namespace SensorX.Warehouse.Application.Events;

[MessageUrn("Product-Created-Event")]
[EntityName("Product-Created-Event")]
public sealed record CreateProductEvent(
    Guid Id,
    string Code,
    string Name,
    string Manufacture,
    string Unit,
    ProductStatus Status,
    DateTimeOffset CreatedAt
);
