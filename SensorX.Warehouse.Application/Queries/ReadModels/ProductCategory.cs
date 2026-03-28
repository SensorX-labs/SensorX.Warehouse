using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Queries.ReadModels;

public class ProductCategory
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
    public string? Description { get; set; }
}
