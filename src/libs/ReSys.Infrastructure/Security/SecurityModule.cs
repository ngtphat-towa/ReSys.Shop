using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Core.Common.Security.Authentication.Tokens;
using ReSys.Core.Common.Security.Authentication.Externals.Interfaces;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Infrastructure.Persistence;
using ReSys.Infrastructure.Security.Authentication.Tokens.Options;
using ReSys.Infrastructure.Security.Authentication.Tokens.Services;
using ReSys.Infrastructure.Security.Authentication.Externals.Options;
using ReSys.Infrastructure.Security.Authentication.Externals.Services;
using ReSys.Infrastructure.Security.Authorization;
using ReSys.Infrastructure.Security.Authorization.Options;
using ReSys.Infrastructure.Security.Authorization.Providers;
using ReSys.Infrastructure.Security.Authorization.Requirements;

namespace ReSys.Infrastructure.Security;

public static class SecurityModule
{
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Identity Core
        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // 2. Options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Section));
        services.Configure<AuthUserCacheOption>(configuration.GetSection(AuthUserCacheOption.Section));
        services.Configure<GoogleOption>(configuration.GetSection(GoogleOption.Section));
        services.Configure<FacebookOption>(configuration.GetSection(FacebookOption.Section));

        // 3. Tokens
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // 4. Authorization
        services.AddScoped<IAuthorizeClaimDataProvider, AuthorizeClaimDataProvider>();
        services.AddSingleton<IAuthorizationPolicyProvider, HasAuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, HasAuthorizeClaimRequirementHandler>();

        // 5. Externals
        services.AddHttpClient<GoogleTokenValidator>();
        services.AddHttpClient<FacebookTokenValidator>();
        services.AddScoped<GoogleTokenValidator>();
        services.AddScoped<FacebookTokenValidator>();
        services.AddScoped<IExternalTokenValidator, CompositeExternalTokenValidator>();
        services.AddScoped<IExternalUserService, ExternalUserService>();

        services.AddAuthorization();

        return services;
    }
}