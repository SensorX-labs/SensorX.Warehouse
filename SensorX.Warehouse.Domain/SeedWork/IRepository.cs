using Ardalis.Specification;
namespace SensorX.Warehouse.Domain.SeedWork;

public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
    // Methods to be used for IUnitOfWork pattern
    public Task Add(T entity, CancellationToken cancellationToken);
    public Task Update(T entity, CancellationToken cancellationToken);
    public Task Delete(T entity, CancellationToken cancellationToken);
    public Task UpdateRange(IEnumerable<T> entities, CancellationToken cancellationToken);
}

