using System.Reflection;
using ReSys.Core.Common.Extensions;

namespace ReSys.Core.Common.Extensions.Query;

internal static class QueryHelper
{
    internal static object? GetDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    internal static PropertyInfo? GetPropertyCaseInsensitive(Type type, string propertyName)
    {
        var pascalName = propertyName.ToPascalCase();
        return type.GetProperty(pascalName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
               ?? type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }
}
