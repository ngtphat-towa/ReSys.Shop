using System.Linq.Expressions;
using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.Common.Extensions;

/// <summary>
/// Provides extension methods for dynamic filtering of IQueryable collections using a string-based DSL.
/// </summary>
public static class FilterExtensions
{
    /// <summary>
    /// Applies a dynamic filter string to the query.
    /// Supports AND (,), OR (|), Grouping (()), and various operators (=, !=, >, <, >=, <=, *, ^, $).
    /// </summary>
    /// <example>
    /// <code>
    /// // Simple filter
    /// query.ApplyDynamicFilter("Status=Active");
    /// 
    /// // Complex logic with grouping and nested properties
    /// query.ApplyDynamicFilter("Price>100,(Category.Name=Electronics|Category.Name=Books)");
    /// 
    /// // Wildcards (Contains, StartsWith, EndsWith)
    /// query.ApplyDynamicFilter("Name*apple*,Description^Start,Code$End");
    /// </code>
    /// </example>
    public static IQueryable<T> ApplyDynamicFilter<T>(this IQueryable<T> query, string? filterString)
    {
        if (string.IsNullOrWhiteSpace(filterString)) return query;

        try 
        {
            // Code Flow:
            // 1. Create a parameter 'x' representing the entity type T.
            // 2. Initialize the recursive descent parser with the filter string.
            // 3. Parse the string into a LINQ Expression tree.
            // 4. Wrap the expression in a Lambda and apply it to the Queryable.Where method.
            var param = Expression.Parameter(typeof(T), "x");
            var parser = new FilterParser<T>(filterString, param);
            var body = parser.Parse();
            
            if (body == null) return query;
            
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }
        catch (Exception)
        {
            // Silently fail and return original query if parsing fails to maintain system stability
            return query; 
        }
    }

    /// <summary>
    /// Internal recursive descent parser for the filter DSL.
    /// Grammar:
    /// OR      -> AND { '|' AND }
    /// AND     -> Factor { ',' Factor }
    /// Factor  -> '(' OR ')' | Condition
    /// Condition -> Field Operator Value
    /// </summary>
    private class FilterParser<T>(string input, ParameterExpression param)
    {
        private readonly string _input = input;
        private int _pos = 0;
        private readonly ParameterExpression _param = param;

        public Expression? Parse() => ParseOr();

        private Expression? ParseOr()
        {
            var left = ParseAnd();
            while (_pos < _input.Length && _input[_pos] == '|')
            {
                _pos++;
                var right = ParseAnd();
                if (left != null && right != null)
                    left = Expression.OrElse(left, right);
                else
                    left = left ?? right; 
            }
            return left;
        }

        private Expression? ParseAnd()
        {
            var left = ParseFactor();
            while (_pos < _input.Length && _input[_pos] == ',')
            {
                _pos++;
                var right = ParseFactor();
                if (left != null && right != null)
                    left = Expression.AndAlso(left, right);
                else
                    left = left ?? right;
            }
            return left;
        }

        private Expression? ParseFactor()
        {
            SkipWhitespace();
            if (_pos < _input.Length && _input[_pos] == '(')
            {
                _pos++;
                var expr = ParseOr();
                if (_pos < _input.Length && _input[_pos] == ')')
                    _pos++;
                return expr;
            }
            return ParseCondition();
        }

        private Expression? ParseCondition()
        {
            SkipWhitespace();
            var start = _pos;
            
            // Extract field name (supports nested paths like Category.Name)
            while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_' || _input[_pos] == '.')) _pos++;
            var field = _input.Substring(start, _pos - start);
            
            if (string.IsNullOrEmpty(field)) return null;

            SkipWhitespace();
            
            // Operator matching
            string? op = null;
            if (Match("!=")) op = "!=";
            else if (Match(">=")) op = ">=";
            else if (Match("<=")) op = "<=";
            else if (Match("!*")) op = "!*";
            else if (Match("=")) op = "=";
            else if (Match(">")) op = ">";
            else if (Match("<")) op = "<";
            else if (Match("^")) op = "^";
            else if (Match("$")) op = "$";
            else if (Match("*")) op = "*";
            
            if (op == null) return null;

            SkipWhitespace();

            // Extract value until separator or end of group
            var valStart = _pos;
            while (_pos < _input.Length && _input[_pos] != ',' && _input[_pos] != '|' && _input[_pos] != ')') _pos++;
            var value = _input.Substring(valStart, _pos - valStart);

            // Shorthand support for wildcards in '=' operator
            if (op == "=")
            {
                if (value.StartsWith("*") && value.EndsWith("*"))
                {
                    op = "*";
                    value = value.Trim('*');
                }
                else if (value.StartsWith("*"))
                {
                     op = "$";
                     value = value.TrimStart('*');
                }
                else if (value.EndsWith("*"))
                {
                     op = "^";
                     value = value.TrimEnd('*');
                }
            }

            return BuildExpression(field, op, value);
        }

        private bool Match(string token)
        {
            if (_pos + token.Length <= _input.Length && _input.Substring(_pos, token.Length) == token)
            {
                _pos += token.Length;
                return true;
            }
            return false;
        }

        private void SkipWhitespace() { while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos])) _pos++; }

        /// <summary>
        /// Builds a LINQ Expression for a single condition, including null-safe navigation for nested properties.
        /// </summary>
        private Expression? BuildExpression(string fieldName, string op, string value)
        {
            Expression expr = _param;
            Type type = typeof(T);
            Expression? nullCheck = null;

            // Handle nested properties (e.g., "Category.Name")
            var members = fieldName.Split('.');
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var property = QueryHelper.GetPropertyCaseInsensitive(type, member);
                if (property == null) return null;

                expr = Expression.Property(expr, property);
                type = property.PropertyType;

                // Add null checks for navigation properties to prevent NullReferenceException in the generated query
                if (i < members.Length - 1)
                {
                    if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
                    {
                        var notNull = Expression.NotEqual(expr, Expression.Constant(null, type));
                        nullCheck = nullCheck == null ? notNull : Expression.AndAlso(nullCheck, notNull);
                    }
                }
            }

            var constExpr = ParseConstant(value, type);
            if (constExpr == null) return null;

            // Special handling for NULL value comparison
            if (constExpr.Value == null)
            {
                switch (op)
                {
                    case "=": return Expression.Equal(expr, constExpr);
                    case "!=": return Expression.NotEqual(expr, constExpr);
                    default: return null; // Other operators not supported for null
                }
            }

            Expression comparison;

            // Type-specific comparison logic
            if (type == typeof(string))
            {
                // Case-insensitive string comparisons
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var startsMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                var endsMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

                var propNotNull = Expression.NotEqual(expr, Expression.Constant(null, typeof(string)));
                
                var paramLower = Expression.Call(expr, toLowerMethod!);
                var constLower = Expression.Constant(constExpr.Value?.ToString()?.ToLower(), typeof(string));
                
                Expression stringComp = null!;
                switch (op)
                {
                    case "*": stringComp = Expression.Call(paramLower, containsMethod!, constLower); break;
                    case "!*": stringComp = Expression.Not(Expression.Call(paramLower, containsMethod!, constLower)); break;
                    case "^": stringComp = Expression.Call(paramLower, startsMethod!, constLower); break;
                    case "$": stringComp = Expression.Call(paramLower, endsMethod!, constLower); break;
                    case "=": stringComp = Expression.Equal(paramLower, constLower); break;
                    case "!=": stringComp = Expression.NotEqual(paramLower, constLower); break;
                    default: return null;
                }
                
                comparison = Expression.AndAlso(propNotNull, stringComp);
            }
            else
            {
                // Standard numeric/date/enum comparisons
                switch (op)
                {
                    case "=": comparison = Expression.Equal(expr, constExpr); break;
                    case "!=": comparison = Expression.NotEqual(expr, constExpr); break;
                    case ">": comparison = Expression.GreaterThan(expr, constExpr); break;
                    case "<": comparison = Expression.LessThan(expr, constExpr); break;
                    case ">=": comparison = Expression.GreaterThanOrEqual(expr, constExpr); break;
                    case "<=": comparison = Expression.LessThanOrEqual(expr, constExpr); break;
                    default: return null;
                }
            }

            // Combine the comparison with any necessary null checks from the navigation path
            if (nullCheck != null)
            {
                return Expression.AndAlso(nullCheck, comparison);
            }
            
            return comparison;
        }

        /// <summary>
        /// Parses a string value into a constant expression of the target type.
        /// Supports Enums, DateTimeOffset, Guid, and primitive types.
        /// </summary>
        private ConstantExpression? ParseConstant(string value, Type targetType)
        {
             if (string.Equals(value, "null", StringComparison.OrdinalIgnoreCase))
             {
                 return Expression.Constant(null, targetType);
             }

             var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
             try {
                if (underlyingType.IsEnum)
                {
                    var val = Enum.Parse(underlyingType, value, true);
                    return Expression.Constant(val, targetType); 
                }
                if (underlyingType == typeof(DateTimeOffset)) 
                {
                    var val = DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);
                    return Expression.Constant(val, targetType);
                }
                if (underlyingType == typeof(Guid)) 
                    return Expression.Constant(Guid.Parse(value), targetType);

                var converted = Convert.ChangeType(value, underlyingType);
                return Expression.Constant(converted, targetType);
             } catch { return null; }
        }
    }
}