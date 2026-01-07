using System.Linq.Expressions;
using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.Common.Extensions;

public static class SearchExtensions
{
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, string? searchText, params string[]? SearchField)
    {
        if (string.IsNullOrWhiteSpace(searchText) || SearchField == null || SearchField.Length == 0)
            return query;

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

            // Ensure we are working with a string for comparison
            Expression stringExpr = propExpr;
            if (type != typeof(string))
            {
                var toStringMethod = typeof(object).GetMethod("ToString");
                stringExpr = Expression.Call(propExpr, toStringMethod!);
            }

            Expression? pathNullChecks = null;
            Expression checkExpr = param;
            var pathType = typeof(T);
            var parts = field.Split('.');
            
            for (int i = 0; i < parts.Length - 1; i++)
            {
                 var property = QueryHelper.GetPropertyCaseInsensitive(pathType, parts[i])!;
                 checkExpr = Expression.Property(checkExpr, property);
                 pathType = property.PropertyType;
                 
                 if (!pathType.IsValueType || Nullable.GetUnderlyingType(pathType) != null)
                 {
                     var notNull = Expression.NotEqual(checkExpr, Expression.Constant(null, pathType));
                     pathNullChecks = pathNullChecks == null ? notNull : Expression.AndAlso(pathNullChecks, notNull);
                 }
            }
            
            // Final property null check if it's a reference type or nullable
            if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
            {
                var finalNotNull = Expression.NotEqual(propExpr, Expression.Constant(null, type));
                pathNullChecks = pathNullChecks == null ? finalNotNull : Expression.AndAlso(pathNullChecks, finalNotNull);
            }

            var lower = Expression.Call(stringExpr, toLower!);
            var contains = Expression.Call(lower, method!, termConstant);
            
            var condition = pathNullChecks == null ? (Expression)contains : Expression.AndAlso(pathNullChecks, contains);

            orBody = orBody == null ? condition : Expression.OrElse(orBody, condition);
        }

        if (orBody == null) return query;

        var lambda = Expression.Lambda<Func<T, bool>>(orBody, param);
        return query.Where(lambda);
    }
}
