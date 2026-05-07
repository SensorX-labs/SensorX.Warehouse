using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.StartPickingNote;

public class StartPickingNoteCommand : IRequest<Result<Guid>>
{
    public Guid PickingNoteId { get; set; }
}