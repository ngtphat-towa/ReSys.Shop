using System.Text.Json;

using ErrorOr;

using FluentValidation;

namespace ReSys.Core.Domain.Common.Abstractions;

public interface IHasMetadata
{
    IDictionary<string, object?> PublicMetadata { get; }
    IDictionary<string, object?> PrivateMetadata { get; }
}

// Constraints:
public static class HasMetadataConstraints
{
    public const int MaxMetadataItems = 50;
    public const int MaxMetadataKeyLength = 100;
    public const int MaxMetadataValueLength = 500;
    public const int MaxTotalMetadataSizeInBytes = 10000;
}

// Predefined Errors:
public static class MetadataErrors
{
    public static Error TooManyItems => Error.Validation(
        code: "Metadata.TooManyItems",
        description: $"Metadata cannot have more than {HasMetadataConstraints.MaxMetadataItems} items.");

    public static Error KeyRequired => Error.Validation(
        code: "Metadata.KeyRequired",
        description: "Metadata key is required.");

    public static Error KeyTooLong => Error.Validation(
        code: "Metadata.KeyTooLong",
        description: $"Metadata key cannot exceed {HasMetadataConstraints.MaxMetadataKeyLength} characters.");

    public static Error ValueTooLong => Error.Validation(
        code: "Metadata.ValueTooLong",
        description: $"Metadata string value cannot exceed {HasMetadataConstraints.MaxMetadataValueLength} characters.");

    public static Error InvalidType => Error.Validation(
        code: "Metadata.InvalidType",
        description: "Metadata values must be simple types (string, int, bool, double, or JsonElement).");

    public static Error TotalSizeExceeded => Error.Validation(
        code: "Metadata.TotalSizeExceeded",
        description: $"Total metadata size exceeds maximum of {HasMetadataConstraints.MaxTotalMetadataSizeInBytes} bytes.");
}

public static class HasMetadataExtensions
{
    // Common Dictionary Methods:
    public static T? GetMetadataValue<T>(this IDictionary<string, object?> metadata, string key, T? defaultValue = default)
    {
        if (metadata.TryGetValue(key, out var value))
        {
            if (value is T typedValue)
                return typedValue;

            if (value is JsonElement element)
                return element.ValueKind == JsonValueKind.Null ? defaultValue : element.Deserialize<T>();
        }

        return defaultValue;
    }

    public static void SetMetadataValue(this IDictionary<string, object?> metadata, string key, object? value)
    {
        if (value == null)
            metadata.Remove(key);
        else
            metadata[key] = value;
    }

    // IHasMetadata Specific Methods:
    public static T? GetPublicMetadata<T>(this IHasMetadata holder, string key, T? defaultValue = default)
        => holder.PublicMetadata.GetMetadataValue(key, defaultValue);

    public static T? GetPrivateMetadata<T>(this IHasMetadata holder, string key, T? defaultValue = default)
        => holder.PrivateMetadata.GetMetadataValue(key, defaultValue);

    public static void SetPublicMetadata(this IHasMetadata holder, string key, object? value)
        => holder.PublicMetadata.SetMetadataValue(key, value);

    public static void SetPrivateMetadata(this IHasMetadata holder, string key, object? value)
        => holder.PrivateMetadata.SetMetadataValue(key, value);

    public static void ClearAllMetadata(this IHasMetadata holder)
    {
        holder.PublicMetadata.Clear();
        holder.PrivateMetadata.Clear();
    }

    public static bool HasKey(this IHasMetadata holder, string key)
        => holder.PublicMetadata.ContainsKey(key) || holder.PrivateMetadata.ContainsKey(key);

    public static bool MetadataEquals(this IDictionary<string, object?>? dict1, IDictionary<string, object?>? dict2)
    {
        if (dict1 == null && dict2 == null) return true;
        if (dict1 == null || dict2 == null) return false;
        if (dict1.Count != dict2.Count) return false;

        var json1 = JsonSerializer.Serialize(dict1);
        var json2 = JsonSerializer.Serialize(dict2);
        return json1 == json2;
    }

    // Validate: dictionary metadata 
    public static void AddMetadataValidationRules<T>(
        this AbstractValidator<T> validator,
        System.Linq.Expressions.Expression<System.Func<T, IDictionary<string, object?>?>> expression)
    {
        validator.RuleFor(expression).AddMetadataValidationRules();
    }

    public static void AddMetadataValidationRules<T>(
        this IRuleBuilder<T, IDictionary<string, object?>?> ruleBuilder)
    {
        ruleBuilder.Custom((dict, context) =>
        {
            if (dict == null) return;

            if (dict.Count > HasMetadataConstraints.MaxMetadataItems)
            {
                context.AddFailure(MetadataErrors.TooManyItems.Description);
            }

            long totalSize = 0;
            foreach (var kvp in dict)
            {
                totalSize += kvp.Key.Length;

                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    context.AddFailure(MetadataErrors.KeyRequired.Description);
                }
                else if (kvp.Key.Length > HasMetadataConstraints.MaxMetadataKeyLength)
                {
                    context.AddFailure(MetadataErrors.KeyTooLong.Description);
                }

                if (kvp.Value != null)
                {
                    if (kvp.Value is string s)
                    {
                        if (s.Length > HasMetadataConstraints.MaxMetadataValueLength)
                        {
                            context.AddFailure(MetadataErrors.ValueTooLong.Description);
                        }
                        totalSize += s.Length;
                    }
                    else if (kvp.Value is int or bool or double or JsonElement)
                    {
                        totalSize += 8; // Constant size for simple types
                    }
                    else
                    {
                        context.AddFailure(MetadataErrors.InvalidType.Description);
                    }
                }
            }

            if (totalSize > HasMetadataConstraints.MaxTotalMetadataSizeInBytes)
            {
                context.AddFailure(MetadataErrors.TotalSizeExceeded.Description);
            }
        });
    }

}