namespace SensorX.Warehouse.Application.Queries.ReadModels;

public record ProductAttribute(
    Guid Id,
    Guid ProductId,
    string AttributeName,
    string AttributeValue
);