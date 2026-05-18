using SensorX.Warehouse.Domain.SeedWork;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Domain.AggregatesModel.ProductAggregate;

public class ProductReadModel : Entity<ProductId>, IAggregateRoot
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Unit { get; private set; }
    public string Manufacture { get; private set; }
    public string Status { get; private set; }
    public DateTimeOffset LastSyncAt { get; private set; }

    public ProductReadModel(ProductId id, string code, string name, string unit, string manufacture, string status) : base(id)
    {
        Code = code;
        Name = name;
        Unit = unit;
        Manufacture = manufacture;
        Status = status;
        LastSyncAt = DateTimeOffset.UtcNow;
    }

    public void Update(string code, string name, string unit, string manufacture, string status)
    {
        Code = code;
        Name = name;
        Unit = unit;
        Manufacture = manufacture;
        Status = status;
        LastSyncAt = DateTimeOffset.UtcNow;
    }
}
