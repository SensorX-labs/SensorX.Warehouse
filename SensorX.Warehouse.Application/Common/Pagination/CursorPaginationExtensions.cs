using System.Linq.Expressions;

namespace SensorX.Warehouse.Application.Common.Pagination;

public static class CursorPaginationExtensions
{
    public static IQueryable<T> ApplyCursorPagination<T>(
        this IQueryable<T> query,
        CursorPagedQuery request,
        Expression<Func<T, DateTimeOffset>> createdAtSelector,
        Expression<Func<T, Guid>> idSelector)
    {
        var param = createdAtSelector.Parameters[0];

        var createdAt = createdAtSelector.Body;
        var id = ReplaceParameter(idSelector.Body, idSelector.Parameters[0], param);

        if (request.IsPrevious && request.FirstCreatedAt.HasValue && request.FirstId.HasValue)
        {
            var predicate = BuildPrevious<T>(
                param,
                createdAt,
                id,
                request.FirstCreatedAt.Value,
                request.FirstId.Value);

            return query.Where(predicate);
        }

        if (request.LastCreatedAt.HasValue && request.LastId.HasValue)
        {
            var predicate = BuildNext<T>(
                param,
                createdAt,
                id,
                request.LastCreatedAt.Value,
                request.LastId.Value);

            return query.Where(predicate);
        }

        return query;
    }

    private static Expression<Func<T, bool>> BuildPrevious<T>(
        ParameterExpression param,
        Expression createdAt,
        Expression id,
        DateTimeOffset firstCreatedAt,
        Guid firstId)
    {
        var body =
            Expression.OrElse(
                Expression.GreaterThan(createdAt, Expression.Constant(firstCreatedAt)),
                Expression.AndAlso(
                    Expression.Equal(createdAt, Expression.Constant(firstCreatedAt)),
                    Expression.GreaterThan(id, Expression.Constant(firstId))
                )
            );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    private static Expression<Func<T, bool>> BuildNext<T>(
        ParameterExpression param,
        Expression createdAt,
        Expression id,
        DateTimeOffset lastCreatedAt,
        Guid lastId)
    {
        var body =
            Expression.OrElse(
                Expression.LessThan(createdAt, Expression.Constant(lastCreatedAt)),
                Expression.AndAlso(
                    Expression.Equal(createdAt, Expression.Constant(lastCreatedAt)),
                    Expression.LessThan(id, Expression.Constant(lastId))
                )
            );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    private static Expression ReplaceParameter(
        Expression body,
        ParameterExpression oldParam,
        ParameterExpression newParam)
    {
        return new ReplaceVisitor(oldParam, newParam).Visit(body);
    }

    private class ReplaceVisitor(
        ParameterExpression oldParam,
        ParameterExpression newParam
    ) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}