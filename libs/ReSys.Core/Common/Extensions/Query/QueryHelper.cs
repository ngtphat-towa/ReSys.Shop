using System.Reflection;

namespace ReSys.Core.Common.Extensions.Query;

/// <summary>
/// Internal helper utilities for reflection-based property access and value handling.
/// </summary>
internal static class QueryHelper
{
    /// <summary>
    /// Returns the default value for a given type.
    /// Returns null for reference types and the initialized value for value types.
    /// </summary>
    internal static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Retrieves a PropertyInfo by name from a type using case-insensitive matching.
    /// Attempts both PascalCase conversion and raw name matching.
    /// </summary>
    internal static PropertyInfo? GetPropertyCaseInsensitive(Type type, string propertyName)
    {
        var pascalName = propertyName.ToPascalCase();
        return type.GetProperty(pascalName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
               ?? type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }
}