using System.Linq.Expressions;
using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.Common.Extensions;

/// <summary>
/// Provides extension methods for global searching across multiple fields.
/// Supports nested properties and automatic string conversion for non-string fields.
/// </summary>
public static class SearchExtensions
{
    /// <summary>
    /// Applies a text search across multiple fields using OR logic.
    /// Non-string fields are automatically converted to string via .ToString().
    /// Includes null-safe navigation for nested property paths.
    /// </summary>
    /// <example>
    /// <code>
    /// // Search across Name, Category Name, and even the Price (converted to string)
    /// query.ApplySearch("apple", "Name", "Category.Name", "Price");
    /// </code>
    /// </example>
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchText, params string[]? SearchField)
    {
        if (string.IsNullOrWhiteSpace(searchText) || SearchField == null || SearchField.Length == 0)
            return query;

        // Code Flow:
        // 1. Create parameter 'x'.
        // 2. Prepare common methods (ToLower, Contains, ToString).
        // 3. Loop through each field to build a search condition.
        // 4. Combine all field conditions with OR.
        // 5. Apply the final expression to the Where clause.

        var param = Expression.Parameter(typeof(T), "x");
        Expression? orBody = null;
        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

        var termConstant = Expression.Constant(searchText.ToLower(), typeof(string));

        foreach (var field in SearchField)
        {
            var type = typeof(T);
            Expression propExpr = param;
            bool validField = true;

            // Build property access path (e.g., x.Category.Name)
            foreach (var member in field.Split('.'))
            {
                var property = QueryHelper.GetPropertyCaseInsensitive(type, member);
                if (property == null) 
                {
                    validField = false;
                    break;
                }

                propExpr = Expression.Property(propExpr, property);
                type = property.PropertyType;
            }

            if (!validField) continue;

            // 1. Handle non-string types by calling .ToString()
            Expression stringExpr = propExpr;
            if (type != typeof(string))
            {
                var toStringMethod = typeof(object).GetMethod("ToString");
                stringExpr = Expression.Call(propExpr, toStringMethod!);
            }

            // 2. Build null-safe path checks
            Expression? pathNullChecks = null;
            Expression checkExpr = param;
            var pathType = typeof(T);
            var parts = field.Split('.');
            
            for (int i = 0; i < parts.Length - 1; i++)
            {
                 var property = QueryHelper.GetPropertyCaseInsensitive(pathType, parts[i])!;
                 checkExpr = Expression.Property(checkExpr, property);
                 pathType = property.PropertyType;
                 
                 // If any segment is a reference type or nullable, ensure it's not null before accessing its child
                 if (!pathType.IsValueType || Nullable.GetUnderlyingType(pathType) != null)
                 {
                     var notNull = Expression.NotEqual(checkExpr, Expression.Constant(null, pathType));
                     pathNullChecks = pathNullChecks == null ? notNull : Expression.AndAlso(pathNullChecks, notNull);
                 }
            }
            
            // 3. Check the final property itself for null
            if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
            {
                var finalNotNull = Expression.NotEqual(propExpr, Expression.Constant(null, type));
                pathNullChecks = pathNullChecks == null ? finalNotNull : Expression.AndAlso(pathNullChecks, finalNotNull);
            }

            // 4. Build case-insensitive "Contains" condition
            var lower = Expression.Call(stringExpr, toLower!);
            var contains = Expression.Call(lower, method!, termConstant);
            
            var condition = pathNullChecks == null ? (Expression)contains : Expression.AndAlso(pathNullChecks, contains);

            // 5. Combine with other fields using OR
            orBody = orBody == null ? condition : Expression.OrElse(orBody, condition);
        }

        if (orBody == null) return query;

        var lambda = Expression.Lambda<Func<T, bool>>(orBody, param);
        return query.Where(lambda);
    }
}