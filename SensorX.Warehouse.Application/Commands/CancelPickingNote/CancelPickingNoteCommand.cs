using MediatR;
using SensorX.Warehouse.Application.Common.ResponseClient;

namespace SensorX.Warehouse.Application.Commands.CancelPickingNote;

public class CancelPickingNoteCommand : IRequest<Result<Guid>>
{
    public Guid PickingNoteId { get; set; }
}