using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SensorX.Warehouse.Application.Common.Interfaces;

namespace SensorX.Warehouse.Infrastructure.Persistences;

public class QueryExecutor(AppDbContext dbContext) : IQueryExecutor
{
    public Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
        => query.ToListAsync(cancellationToken);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
        => query.FirstOrDefaultAsync(cancellationToken);

    public Task<T> FirstAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
        => query.FirstAsync(cancellationToken);

    public Task<T?> SingleOrDefaultAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
        => query.SingleOrDefaultAsync(cancellationToken);

    public Task<T> SingleAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
        => query.SingleAsync(cancellationToken);

    public Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
        => query.CountAsync(cancellationToken);

    public Task<bool> AnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
        => query.AnyAsync(cancellationToken);

    public Task<bool> AllAsync<T>(IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => query.AllAsync(predicate, cancellationToken);
}