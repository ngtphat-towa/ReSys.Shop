using Carter;

using ReSys.Api.Infrastructure;
using ReSys.Api.Infrastructure.Documentation;
using ReSys.Api.Infrastructure.Middleware;
using ReSys.Api.Infrastructure.Serialization;
using ReSys.Core.Common.Telemetry;
using ReSys.Core.Common.Constants;

using Serilog;


using OpenIddict.Validation.AspNetCore;

namespace ReSys.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule("Presentation", "API", s =>
        {
            s.AddCustomSerialization() 
             .AddDocumentation()
             .AddCustomAuthentication(configuration)
             .AddCors(options =>
             {
                 options.AddDefaultPolicy(policy =>
                 {
                     policy.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                 });
             });

            s.AddCarter();

            s.AddExceptionHandler<GlobalExceptionHandler>();
            s.AddProblemDetails();
        });

        return services;
    }

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        services.AddOpenIddict()
            .AddValidation(options =>
            {
                var authority = configuration["Authentication:Authority"];
                if (!string.IsNullOrEmpty(authority))
                {
                    options.SetIssuer(authority);
                }
                
                options.AddAudiences(AuthConstants.Resources.ShopApi);
                
                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        services.AddAuthorization();

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        // 1. Safety Net (Exception Handling)
        app.UseExceptionHandler();

        // 2. Structured Request Logging (Serilog)
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        // 3. Health Checks
        app.MapDefaultEndpoints();

        // 4. Normalization & Security
        app.UseMiddleware<SnakeCaseQueryMiddleware>();
        app.UseDocumentation();

        app.UseHttpsRedirection();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        // 5. Execution (Vertical Slices)
        app.MapCarter();

        return app;
    }
}
