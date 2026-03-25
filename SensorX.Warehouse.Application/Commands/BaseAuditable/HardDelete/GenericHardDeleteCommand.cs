using MediatR;

namespace SensorX.Warehouse.Application.Commands.BaseAuditable.HardDelete
{
    public abstract record GenericHardDeleteCommand(List<Guid> Ids, long UserId) : IRequest<bool>;
}

