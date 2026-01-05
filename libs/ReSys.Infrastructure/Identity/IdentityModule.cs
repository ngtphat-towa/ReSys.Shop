using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using ReSys.Core.Common.Security;
using ReSys.Core.Domain.Identity;
using ReSys.Infrastructure.Persistence;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Infrastructure.Identity;

public static class IdentityModule
{
    /// <summary>
    /// Registers the core Identity services (EF Core stores, User/Role Managers).
    /// Used by BOTH Identity Service and API (if API needs direct DB access to users).
    /// </summary>
    public static IServiceCollection AddIdentityStorage(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUser, WebUser>();

        // 1. Setup ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            
            options.User.RequireUniqueEmail = true;
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
            options.ClaimsIdentity.EmailClaimType = Claims.Email;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    /// <summary>
    /// Registers the OpenIddict SERVER components. 
    /// ONLY used by the Identity Service.
    /// </summary>
    public static IServiceCollection AddIdentityServer(this IServiceCollection services)
    {
        // 2. Setup OpenIddict
        services.AddOpenIddict()

            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<AppDbContext>();
            })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the authorization, logout, token and userinfo endpoints.
                options.SetAuthorizationEndpointUris("connect/authorize")
                       .SetEndSessionEndpointUris("connect/logout")
                       .SetTokenEndpointUris("connect/token")
                       .SetUserInfoEndpointUris("connect/userinfo");

                // Enable the client credentials flow.
                options.AllowClientCredentialsFlow();

                // Enable the authorization code flow.
                options.AllowAuthorizationCodeFlow()
                       .RequireProofKeyForCodeExchange();

                // Enable the refresh token flow.
                options.AllowRefreshTokenFlow();

                // Register the scopes (permissions).
                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                // Encryption and Signing Credentials
                // For Production: Use AddEncryptionCertificate/AddSigningCertificate with X509Certificate2 from KeyVault/SecretStore.
                // For Development: The following generates temporary certificates.
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableEndSessionEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserInfoEndpointPassthrough()
                       .EnableStatusCodePagesIntegration();
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        return services;
    }

    /// <summary>
    /// Registers the OpenIddict VALIDATION components (JWT Bearer).
    /// Used by APIs that need to validate tokens but not issue them.
    /// </summary>
    public static IServiceCollection AddIdentityValidation(this IServiceCollection services, IConfiguration configuration)
    {
         // 1. Bind and Validate Options
         services.AddOptions<IdentityValidationOptions>()
             .Configure(options => 
             {
                 // Aspire/Service Discovery convention: services:{name}:http
                 options.Authority = configuration["services:identity:http"] 
                                     ?? configuration["Identity:Url"] // Fallback
                                     ?? string.Empty;
             })
             .ValidateDataAnnotations()
             .ValidateOnStart();

         // 2. Register OpenIddict Validation
         services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Configuration is handled by ConfigureIdentityValidationOptions
                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });
         
         services.AddSingleton<IConfigureOptions<OpenIddict.Validation.OpenIddictValidationOptions>, ConfigureIdentityValidationOptions>();
         
         services.AddAuthentication(OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
         services.AddAuthorization();

         return services;
    }
}