using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.CompletePickingNote;

public class CompletePickingNoteCommand : IRequest<Result<Guid>>
{
    public Guid PickingNoteId { get; set; }
}