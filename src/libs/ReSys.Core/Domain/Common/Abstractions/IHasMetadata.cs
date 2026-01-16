using System.Text.Json;

namespace ReSys.Core.Domain.Common.Abstractions;

public interface IHasMetadata
{
    IDictionary<string, object?> PublicMetadata { get; }
    IDictionary<string, object?> PrivateMetadata { get; }
}

public static class HasMetadataExtensions
{
    public static T? GetPublicMetadata<T>(this IHasMetadata holder, string key, T? defaultValue = default)
    {
        if (holder.PublicMetadata.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        
        if (value is JsonElement element)
            return element.Deserialize<T>();

        return defaultValue;
    }

    public static T? GetPrivateMetadata<T>(this IHasMetadata holder, string key, T? defaultValue = default)
    {
        if (holder.PrivateMetadata.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;

        if (value is JsonElement element)
            return element.Deserialize<T>();

        return defaultValue;
    }

    public static void SetPublicMetadata(this IHasMetadata holder, string key, object? value)
    {
        if (value == null)
            holder.PublicMetadata.Remove(key);
        else
            holder.PublicMetadata[key] = value;
    }

    public static void SetPrivateMetadata(this IHasMetadata holder, string key, object? value)
    {
        if (value == null)
            holder.PrivateMetadata.Remove(key);
        else
            holder.PrivateMetadata[key] = value;
    }

    public static bool MetadataEquals(this IDictionary<string, object?>? dict1, IDictionary<string, object?>? dict2)
    {
        if (dict1 == null && dict2 == null) return true;
        if (dict1 == null || dict2 == null) return false;
        if (dict1.Count != dict2.Count) return false;

        var json1 = JsonSerializer.Serialize(dict1);
        var json2 = JsonSerializer.Serialize(dict2);
        return json1 == json2;
    }
}