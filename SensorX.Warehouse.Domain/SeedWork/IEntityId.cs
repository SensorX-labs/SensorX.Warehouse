namespace SensorX.Warehouse.Domain.SeedWork;

public interface IEntityId<TSelf> where TSelf : IEntityId<TSelf>
{
    // Định nghĩa hàm static abstract: Bắt buộc lớp con phải override
    static abstract TSelf New();
    Guid Value { get; init; }
}