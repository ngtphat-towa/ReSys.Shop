using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using ReSys.Core.Common.Constants;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence;
using ReSys.Identity.Persistence.Constants;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Identity.Seeding;

public class IdentitySeeder(IServiceProvider serviceProvider, ILogger<IdentitySeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        
        logger.LogInformation(LogTemplates.Bootstrapper.Starting, "IdentitySeeder", "Database Migration & Seeding");

        // Use MigrateAsync instead of EnsureCreated to support schemas and evolution
        try 
        {
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation(LogTemplates.Bootstrapper.Configuring, "Identity Database", "Migrations Applied Successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply Identity migrations.");
            throw;
        }

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // 0. Seed Claim Definitions (Permissions)
        logger.LogInformation(LogTemplates.Bootstrapper.Configuring, "Claim Definitions", "Seeding");
        foreach (var def in AppPermissions.All)
        {
            if (!context.ClaimDefinitions.Any(x => x.Type == def.Type && x.Value == def.Value))
            {
                context.ClaimDefinitions.Add(new ClaimDefinition
                {
                    Type = def.Type,
                    Value = def.Value,
                    Description = def.Description,
                    Category = def.Category
                });
            }
        }
        await context.SaveChangesAsync(cancellationToken);

        // 1. Seed Roles
        string[] roles = [AuthConstants.Roles.Admin, AuthConstants.Roles.Customer];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    logger.LogInformation(LogTemplates.Domain.EntityCreated, "Role", role, "System");
                }
                else 
                {
                    logger.LogError(LogTemplates.Domain.OperationFailed, $"Create Role {role}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // 0.1 Seed Default Admin
        var adminEmail = "admin@resys.shop";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                logger.LogInformation(LogTemplates.Domain.EntityCreated, "User", adminEmail, "System");
                await userManager.AddToRoleAsync(adminUser, AuthConstants.Roles.Admin);
            }
            else
            {
                logger.LogError(LogTemplates.Domain.OperationFailed, $"Create Admin User {adminEmail}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 0.2 Seed Scopes from AppPermissions
        foreach (var permission in AppPermissions.All)
        {
            if (await scopeManager.FindByNameAsync(permission.Value, cancellationToken) is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = permission.Value,
                    DisplayName = permission.Description,
                    Resources = { AuthConstants.Resources.ShopApi }
                }, cancellationToken);
            }
        }

        // Standard Roles scope
        if (await scopeManager.FindByNameAsync(Scopes.Roles, cancellationToken) is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = Scopes.Roles,
                DisplayName = "Roles",
                Resources = { AuthConstants.Resources.ShopApi }
            }, cancellationToken);
        }

        // 1. Shop Client
        if (await manager.FindByClientIdAsync(AuthConstants.Clients.ShopWeb, cancellationToken) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = AuthConstants.Clients.ShopWeb,
                DisplayName = "ReSys Shop Web",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "offline_access"
                }
            }, cancellationToken);
        }

        // 2. Admin Client
        if (await manager.FindByClientIdAsync(AuthConstants.Clients.AdminWeb, cancellationToken) is null)
        {
            var adminDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = AuthConstants.Clients.AdminWeb,
                DisplayName = "ReSys Admin Web",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "offline_access"
                }
            };

            // Add all application permissions to the Admin Client automatically
            foreach (var permission in AppPermissions.All)
            {
                adminDescriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + permission.Value);
            }

            await manager.CreateAsync(adminDescriptor, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}