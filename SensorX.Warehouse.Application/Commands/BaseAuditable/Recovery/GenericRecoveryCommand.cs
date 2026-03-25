using MediatR;

namespace SensorX.Warehouse.Application.Commands.BaseAuditable.Recovery
{
    public abstract record GenericRecoveryCommand(Guid Id, long UserId) : IRequest<bool>;
}

