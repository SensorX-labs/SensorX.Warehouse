
using SensorX.Warehouse.Domain.SeedWork;
namespace SensorX.Warehouse.Domain.StrongIDs;

public record ProductId(Guid Value) : EntityId<ProductId>(Value);
