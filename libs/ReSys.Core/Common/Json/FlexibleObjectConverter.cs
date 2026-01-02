using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace ReSys.Core.Common.Json;

public class FlexibleObjectConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Apply to classes in the Features namespace (Requests/DTOs)
        return typeToConvert.IsClass && 
               !typeToConvert.IsAbstract && 
               typeToConvert.Namespace != null && 
               typeToConvert.Namespace.Contains(".Features.");
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(
            typeof(FlexibleObjectConverter<>).MakeGenericType(typeToConvert));
    }
}

public class FlexibleObjectConverter<T> : JsonConverter<T> where T : class, new()
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var instance = new T();
        var properties = typeToConvert.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => Normalize(p.Name), p => p, StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return instance;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                if (propertyName != null && properties.TryGetValue(Normalize(propertyName), out var property))
                {
                    var value = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                    property.SetValue(instance, value);
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Use default serialization for responses (which will use the SnakeCase policy from options)
        // We create a new options clone without this converter to avoid infinite recursion
        var clonedOptions = new JsonSerializerOptions(options);
        for (int i = clonedOptions.Converters.Count - 1; i >= 0; i--)
        {
            if (clonedOptions.Converters[i] is FlexibleObjectConverterFactory)
            {
                clonedOptions.Converters.RemoveAt(i);
            }
        }
        JsonSerializer.Serialize(writer, value, clonedOptions);
    }

    private static string Normalize(string name) => name.Replace("_", "").ToLowerInvariant();
}
