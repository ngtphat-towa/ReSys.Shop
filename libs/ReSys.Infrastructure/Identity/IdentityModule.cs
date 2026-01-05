using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                       .SetTokenEndpointUris("connect/token");

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
                // TODO: Replace with real certificates in Production
                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
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
    public static IServiceCollection AddIdentityValidation(this IServiceCollection services, string authorityUrl)
    {
         services.AddOpenIddict()
            .AddValidation(options =>
            {
                // Note: The issuer URI must match the one from the discovery document.
                options.SetIssuer(authorityUrl);
                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });
         
         services.AddAuthentication(OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
         services.AddAuthorization();

         return services;
    }
}