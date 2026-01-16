using System.Linq.Expressions;

using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.Common.Extensions.Sort;

/// <summary>
/// Provides extension methods for dynamic sorting of IQueryable collections using property names.
/// </summary>
public static class SortExtensions
{
    /// <summary>
    /// Applies a single dynamic sort to the query.
    /// </summary>
    public static IQueryable<T> ApplyDynamicSort<T>(this IQueryable<T> query, string? sortBy, bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return query;

        var param = Expression.Parameter(typeof(T), "x");
        Expression body = param;
        var type = typeof(T);

        // Code Flow:
        // 1. Traverse property path (e.g., "Category.Name").
        // 2. Add null checks for navigation segments.
        // 3. Build the OrderBy or OrderByDescending call via Reflection.

        foreach (var member in sortBy.Split('.'))
        {
            var property = QueryHelper.GetPropertyCaseInsensitive(type, member);
            if (property == null) return query;

            var nextBody = Expression.Property(body, property);

            // Apply null-safe condition for navigation
            if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
            {
                var defaultVal = Expression.Constant(QueryHelper.GetDefault(property.PropertyType), property.PropertyType);
                body = Expression.Condition(
                    Expression.Equal(body, Expression.Constant(null, type)),
                    defaultVal,
                    nextBody);
            }
            else
            {
                body = nextBody;
            }

            type = property.PropertyType;
        }

        var lambda = Expression.Lambda(body, param);

        var methodName = isDescending ? "OrderByDescending" : "OrderBy";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Applies multiple sorts from a comma-separated string.
    /// Supports "FieldName" (asc) and "FieldName desc" syntax.
    /// </summary>
    /// <example>
    /// <code>
    /// // Sort by price highest first, then by name alphabetically
    /// query.ApplyDynamicOrdering("Price desc, Name asc");
    /// </code>
    /// </example>
    public static IQueryable<T> ApplyDynamicOrdering<T>(this IQueryable<T> query, string? sortString)
    {
        if (string.IsNullOrWhiteSpace(sortString)) return query;

        var orderedQuery = query;
        bool isFirst = true;

        foreach (var part in sortString.Split(','))
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // 1. Detect direction
            var isDescending = trimmed.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
            var propertyName = isDescending ? trimmed.Substring(0, trimmed.Length - 5).Trim() : trimmed.Trim();

            if (propertyName.EndsWith(" asc", StringComparison.OrdinalIgnoreCase))
            {
                propertyName = propertyName.Substring(0, propertyName.Length - 4).Trim();
            }

            // 2. Build property access expression
            var param = Expression.Parameter(typeof(T), "x");
            Expression body = param;
            var type = typeof(T);

            foreach (var member in propertyName.Split('.'))
            {
                var property = QueryHelper.GetPropertyCaseInsensitive(type, member);
                if (property == null) goto NextPart;

                var nextBody = Expression.Property(body, property);

                if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
                {
                    var defaultVal = Expression.Constant(QueryHelper.GetDefault(property.PropertyType), property.PropertyType);
                    body = Expression.Condition(
                        Expression.Equal(body, Expression.Constant(null, type)),
                        defaultVal,
                        nextBody);
                }
                else
                {
                    body = nextBody;
                }

                type = property.PropertyType;
            }

            var lambda = Expression.Lambda(body, param);

            // 3. Determine method (OrderBy vs ThenBy)
            string methodName;
            if (isFirst)
                methodName = isDescending ? "OrderByDescending" : "OrderBy";
            else
                methodName = isDescending ? "ThenByDescending" : "ThenBy";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), type },
                orderedQuery.Expression,
                Expression.Quote(lambda));

            orderedQuery = orderedQuery.Provider.CreateQuery<T>(resultExpression);
            isFirst = false;

        NextPart:;
        }

        return orderedQuery;
    }
}
