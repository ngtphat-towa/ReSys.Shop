using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


using ReSys.Core.Common.Constants;
using ReSys.Core.Common.Interfaces;
using ReSys.Core.Common.Telemetry;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence;
using ReSys.Infrastructure.Authentication;
using ReSys.Infrastructure.Authentication.Authorization;


using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Identity.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();

        services.RegisterModule("Infrastructure", "Authentication", s =>
        {
            s.AddHttpContextAccessor();
            s.AddScoped<IUserContext, UserContext>();
            s.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            s.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            s.Configure<PermissionAuthorizationOptions>(options =>
            {
                options.AuthenticationSchemes.Add(AuthConstants.Schemes.Bearer);
            });

            s.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders()
                .AddSignInManager();

            s.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<AppIdentityDbContext>();
                })
                .AddServer(options =>
                {
                    options.SetTokenEndpointUris("/connect/token");

                    options.AllowPasswordFlow()
                           .AllowRefreshTokenFlow();

                    options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "permissions");

                    options.Configure(opts => opts.RequireProofKeyForCodeExchange = true);

                    options.AcceptAnonymousClients();

                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    options.DisableAccessTokenEncryption();

                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough()
                           .DisableTransportSecurityRequirement();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            s.AddAuthorization(options =>
            {
                // Optionally add global policies here
            });
        });

        return services;
    }
}
