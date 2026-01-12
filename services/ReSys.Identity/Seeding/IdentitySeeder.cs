using OpenIddict.Abstractions;
using ReSys.Identity.Persistence;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Identity.Seeding;

public class IdentitySeeder(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        // Ensure DB created
        await context.Database.EnsureCreatedAsync(cancellationToken);

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // 1. Shop Client
        if (await manager.FindByClientIdAsync("resys-shop-web", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "resys-shop-web",
                DisplayName = "ReSys Shop Web",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Roles,
                    "scp:offline_access" // Short syntax for Permissions.Prefixes.Scope + "offline_access"
                }
            }, cancellationToken);
        }

        // 2. Admin Client
        if (await manager.FindByClientIdAsync("resys-admin-web", cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "resys-admin-web",
                DisplayName = "ReSys Admin Web",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Roles,
                    "scp:offline_access"
                }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}