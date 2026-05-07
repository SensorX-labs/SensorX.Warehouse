using System.Linq.Expressions;

namespace SensorX.Warehouse.Application.Common.Interfaces;

/// <summary>
/// QueryExecutor is responsible for executing IQueryable expressions.
/// It abstracts EF Core (or any ORM) away from Application layer.
/// </summary>
public interface IQueryExecutor
{
    Task<List<T>> ToListAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<T> FirstAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<T?> SingleOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<T> SingleAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    Task<bool> AllAsync<T>(
        IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
}