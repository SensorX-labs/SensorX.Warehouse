namespace SensorX.Warehouse.Application.Queries.ReadModels;

public record ProductImage(
    Guid Id,
    Guid ProductId,
    string ImageUrl
);