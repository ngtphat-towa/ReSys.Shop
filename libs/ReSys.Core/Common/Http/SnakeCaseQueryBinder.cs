using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ReSys.Core.Common.Http;

public static class SnakeCaseQueryBinder
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    public static ValueTask<T?> BindAsync<T>(HttpContext context) where T : class, new()
    {
        var request = new T();
        var type = typeof(T);
        var properties = PropertyCache.GetOrAdd(type, t => 
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanWrite)
             .ToArray());

        var query = context.Request.Query;

        foreach (var prop in properties)
        {
            var snakeKey = ToSnakeCase(prop.Name);
            if (query.TryGetValue(snakeKey, out var values))
            {
                var stringValue = values.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(stringValue)) continue;

                // Log for debugging in tests
                Console.WriteLine($"[SnakeCaseQueryBinder] Found {snakeKey}={stringValue} for {prop.Name}");

                try
                {
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    
                    object? convertedValue = null;

                    if (targetType == typeof(DateTimeOffset))
                    {
                        if (DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
                            convertedValue = dto.ToUniversalTime();
                    }
                    else if (targetType == typeof(Guid))
                    {
                        if (Guid.TryParse(stringValue, out var guid))
                            convertedValue = guid;
                    }
                    else if (targetType == typeof(decimal))
                    {
                        if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                            convertedValue = dec;
                    }
                    else if (targetType == typeof(int))
                    {
                        if (int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var i))
                            convertedValue = i;
                    }
                    else if (targetType == typeof(bool))
                    {
                        if (bool.TryParse(stringValue, out var b))
                            convertedValue = b;
                    }
                    else
                    {
                        var converter = TypeDescriptor.GetConverter(targetType);
                        if (converter.CanConvertFrom(typeof(string)))
                        {
                            convertedValue = converter.ConvertFromInvariantString(stringValue);
                        }
                    }

                    if (convertedValue != null)
                    {
                        prop.SetValue(request, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SnakeCaseQueryBinder] Error binding {snakeKey}: {ex.Message}");
                }
            }
        }

        return ValueTask.FromResult<T?>(request);
    }

    private static string ToSnakeCase(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return Regex.Replace(text, @"([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }
}