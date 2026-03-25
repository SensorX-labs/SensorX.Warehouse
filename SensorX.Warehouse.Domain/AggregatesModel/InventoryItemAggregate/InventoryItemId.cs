using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.AggregatesModel.InventoryItemAggregate;

public record InventoryItemId(Guid Id) : VoId(Id);