using SensorX.Warehouse.Domain.Common.Extensions;
using SensorX.Warehouse.Domain.Common.Specifications;
using SensorX.Warehouse.Domain.SeedWork;
using MediatR;

namespace SensorX.Warehouse.Application.Commands.BaseAuditable.SoftDelete
{
    public abstract class GenericSoftDeleteHandler<TEntity, TCommand>(
        IRepository<TEntity> repository
    ) : IRequestHandler<TCommand, bool>
         where TEntity : Entity, ISoftDeletable, IAggregateRoot
         where TCommand : GenericSoftDeleteCommand
    {
        public async Task<bool> Handle(TCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new EntitiesByIdsSpecification<TEntity>(request.Ids, false);
                var entities = await repository.ListAsync(spec, cancellationToken);
                if (entities == null || entities.Count == 0)
                    throw new ApplicationException("Không tìm thấy bất kỳ bản ghi nào");

                foreach (var entity in entities)
                {
                    entity.MarkDeleted(request.UserId);
                }

                await repository.UpdateRangeAsync(entities, cancellationToken);
                return true;
            }
            catch (ApplicationException ex)
            {
                throw new ApplicationException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Đã xảy ra lỗi khi xóa bản ghi: {ex.Message}");
            }
        }
    }
}

