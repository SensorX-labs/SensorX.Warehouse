namespace SensorX.Warehouse.Domain.SeedWork;

public interface IEntityId<TSelf> where TSelf : IEntityId<TSelf>
{
    static abstract TSelf New();
}