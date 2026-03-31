using SensorX.Warehouse.Domain.ValueObjects;
namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;

public record DocumentReference(
    DocumentType Type,
    Guid Id,
    string Code
);


