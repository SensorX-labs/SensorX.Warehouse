using Microsoft.EntityFrameworkCore;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Infrastructure.Persistences;

public class QueryBuilder<T>(AppDbContext dbContext) : IQueryBuilder<T> where T : class
{
    public IQueryable<T> Query => dbContext.Set<T>();

    public IQueryable<T> QueryAsNoTracking => dbContext.Set<T>().AsNoTracking();
}