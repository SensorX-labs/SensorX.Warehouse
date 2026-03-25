using Ardalis.Specification;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Domain.Common.Specifications
{
    public class EntitiesByIdsSpecification<TEntity> : Specification<TEntity>
        where TEntity : Entity<VoId>, IAggregateRoot, ISoftDeletable
    {
        public EntitiesByIdsSpecification(IEnumerable<VoId> ids, bool? isDeleted = null)
        {
            if (isDeleted.HasValue)
            {
                Query.Where(e => ids.Contains(e.Id) && e.IsDeleted == isDeleted.Value);
            }
            else
            {
                Query.Where(e => ids.Contains(e.Id));
            }
        }
    }
}

