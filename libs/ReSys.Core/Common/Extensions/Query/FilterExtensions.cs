using System.Linq.Expressions;
using ReSys.Core.Common.Extensions.Query;

namespace ReSys.Core.Common.Extensions; // Keep generic namespace for easy usage

public static class FilterExtensions
{
    public static IQueryable<T> ApplyDynamicFilter<T>(this IQueryable<T> query, string? filterString)
    {
        if (string.IsNullOrWhiteSpace(filterString)) return query;

        try 
        {
            var param = Expression.Parameter(typeof(T), "x");
            var parser = new FilterParser<T>(filterString, param);
            var body = parser.Parse();
            
            if (body == null) return query;
            
            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return query.Where(lambda);
        }
        catch (Exception)
        {
            return query; 
        }
    }

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
            
            while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_' || _input[_pos] == '.')) _pos++;
            var field = _input.Substring(start, _pos - start);
            
            if (string.IsNullOrEmpty(field)) return null;

            SkipWhitespace();
            
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

            var valStart = _pos;
            while (_pos < _input.Length && _input[_pos] != ',' && _input[_pos] != '|' && _input[_pos] != ')') _pos++;
            var value = _input.Substring(valStart, _pos - valStart);

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

        private Expression? BuildExpression(string fieldName, string op, string value)
        {
            Expression expr = _param;
            Type type = typeof(T);
            Expression? nullCheck = null;

            var members = fieldName.Split('.');
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var property = QueryHelper.GetPropertyCaseInsensitive(type, member);
                if (property == null) return null;

                expr = Expression.Property(expr, property);
                type = property.PropertyType;

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

            // Special handling for NULL values
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

            if (type == typeof(string))
            {
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

            if (nullCheck != null)
            {
                return Expression.AndAlso(nullCheck, comparison);
            }
            
            return comparison;
        }

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
