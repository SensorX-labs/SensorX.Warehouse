using Ardalis.Specification.EntityFrameworkCore;
using SensorX.Warehouse.Domain.SeedWork;

namespace SensorX.Warehouse.Infrastructure.Persistences;

public class EfRepository<T>(AppDbContext dbContext) : RepositoryBase<T>(dbContext), IRepository<T> where T : class, IAggregateRoot
{
    public void Add(T entity) => DbContext.Set<T>().Add(entity);
    public void Update(T entity) => DbContext.Set<T>().Update(entity);
    public void Delete(T entity) => DbContext.Set<T>().Remove(entity);
}

public class EfReadRepository<T> : RepositoryBase<T>, IReadRepository<T> where T : Entity
{
    public EfReadRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}

