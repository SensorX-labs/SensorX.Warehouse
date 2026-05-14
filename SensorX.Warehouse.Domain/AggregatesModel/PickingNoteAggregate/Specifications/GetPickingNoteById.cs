using Ardalis.Specification;
using SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate;
using SensorX.Warehouse.Domain.StrongIDs;

namespace SensorX.Warehouse.Domain.AggregatesModel.PickingNoteAggregate.Specifications;

public class GetPickingNoteById : SingleResultSpecification<PickingNote>
{
    public GetPickingNoteById(PickingNoteId id)
    {
        Query.Where(x => x.Id == id);
    }
}
