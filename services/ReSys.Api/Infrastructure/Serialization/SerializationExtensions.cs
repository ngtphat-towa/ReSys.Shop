using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ReSys.Api.Infrastructure.Serialization;

public static class SerializationExtensions
{
    public static IServiceCollection AddCustomSerialization(this IServiceCollection services)
    {
        // 1. Newtonsoft snake_case for MVC Controllers (Bodies, etc.)
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc; // Force Z for DateTime
            options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        });

        // 2. System.Text.Json snake_case for Minimal APIs (Results.Ok, AsParameters, etc.)
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new UtcDateTimeOffsetConverter());
            options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

        return services;
    }
}
