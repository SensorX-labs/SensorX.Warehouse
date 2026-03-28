using SensorX.Warehouse.Domain.ValueObjects;

namespace SensorX.Warehouse.Application.Queries.ReadModels;

public class Product
{
    public required ProductId Id { get; set; }
    public required Code Code { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public required string ManufactureName { get; set; }
    public string? Note { get; set; }

    public CategoryId? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    public ICollection<ProductAttribute>? Attributes { get; set; }
    public ICollection<ProductImage>? Images { get; set; }
}