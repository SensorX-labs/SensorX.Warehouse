using Ardalis.Specification;
namespace SensorX.Warehouse.Domain.SeedWork;

public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
    // Methods to be used for IUnitOfWork pattern
    public void Add(T entity);
    public void Update(T entity);
    public void Delete(T entity);
}
