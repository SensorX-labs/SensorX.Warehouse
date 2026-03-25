using MediatR;

namespace SensorX.Warehouse.Application.Commands.BaseAuditable.SoftDelete
{
    public abstract record GenericSoftDeleteCommand(List<Guid> Ids, long UserId) : IRequest<bool>;
}

