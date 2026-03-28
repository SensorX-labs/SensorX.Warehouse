namespace SensorX.Warehouse.Application.Queries.ReadModels;

public record ProductAttribute(
    Guid Id,
    string AttributeName,
    string AttributeValue
);