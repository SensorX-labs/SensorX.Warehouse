using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Queries.ReadModels;

public class ProductCategory
{
    public required CategoryId Id { get; set; }
    public required string Name { get; set; }
    public CategoryId? ParentId { get; set; }
    public string? Description { get; set; }
}
