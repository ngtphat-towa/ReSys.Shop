using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.Permissions;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Domain.Identity.Roles.Claims;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.Roles;
using ReSys.Shared.Constants.Permissions;
using ReSys.Core.Common.Security.Authorization.Claims;
using ErrorOr;

namespace ReSys.Infrastructure.Persistence.Seeders;

public class IdentityDataSeeder(
    IApplicationDbContext dbContext,
    ILogger<IdentityDataSeeder> logger,
    IPasswordHasher<User> passwordHasher) : IDataSeeder
{
    public int Order => 1;

    public async Task<ErrorOr<Success>> SeedAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (await dbContext.Set<User>().AnyAsync(cancellationToken)) 
                return Result.Success;

            logger.LogInformation("Seeding Identity data...");

            // 1. Seed Permissions from Constants
            var permissions = GetFlatPermissions();
            foreach (var pName in permissions)
            {
                if (!await dbContext.Set<AccessPermission>().AnyAsync(x => x.Name == pName, cancellationToken))
                {
                    var pResult = AccessPermission.Create(pName);
                    if (pResult.IsError) return pResult.Errors;
                    
                    dbContext.Set<AccessPermission>().Add(pResult.Value);
                }
            }
            await dbContext.SaveChangesAsync(cancellationToken);

            // 2. Seed Roles
            var adminRoleResult = Role.Create("Admin", "Administrator", "Full system access", 100, isDefault: false, isSystemRole: true);
            if (adminRoleResult.IsError) return adminRoleResult.Errors;

            var customerRoleResult = Role.Create("Customer", "Customer", "Standard storefront user", 0, isDefault: true, isSystemRole: false);
            if (customerRoleResult.IsError) return customerRoleResult.Errors;

            var adminRole = adminRoleResult.Value;
            var customerRole = customerRoleResult.Value;

            dbContext.Set<Role>().AddRange(adminRole, customerRole);
            await dbContext.SaveChangesAsync(cancellationToken);

            // 3. Assign All Permissions to Admin
            foreach (var pName in permissions)
            {
                dbContext.Set<RoleClaim>().Add(new RoleClaim 
                { 
                    RoleId = adminRole.Id, 
                    ClaimType = CustomClaim.Permission, 
                    ClaimValue = pName 
                });
            }
            await dbContext.SaveChangesAsync(cancellationToken);

            // 4. Seed Admin User
            var adminUserResult = User.Create("admin@resys.shop", "admin", "System", "Administrator", emailConfirmed: true);
            if (adminUserResult.IsError) return adminUserResult.Errors;
            
            var adminUser = adminUserResult.Value;
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Seeder@123");
            
            dbContext.Set<User>().Add(adminUser);
            await dbContext.SaveChangesAsync(cancellationToken);

            // 5. Link Admin User to Admin Role
            dbContext.Set<UserRole>().Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Identity data seeded successfully.");
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Identity data.");
            return Error.Failure("Seeder.Identity.Error", ex.Message);
        }
    }

    private static List<string> GetFlatPermissions()
    {
        return new List<string>
        {
            FeaturePermissions.Admin.Identity.User.List,
            FeaturePermissions.Admin.Identity.User.View,
            FeaturePermissions.Admin.Identity.User.Create,
            FeaturePermissions.Admin.Identity.User.Update,
            FeaturePermissions.Admin.Identity.User.Delete,
            FeaturePermissions.Admin.Identity.User.AssignRole,
            FeaturePermissions.Admin.Identity.User.UnassignRole,
            FeaturePermissions.Admin.Identity.Role.List,
            FeaturePermissions.Admin.Identity.Role.View,
            FeaturePermissions.Admin.Identity.Role.Create,
            FeaturePermissions.Admin.Identity.Role.Update,
            FeaturePermissions.Admin.Identity.Role.Delete,
            FeaturePermissions.Admin.Identity.Role.AssignPermission,
            FeaturePermissions.Admin.Identity.Role.UnassignPermission,
            FeaturePermissions.Admin.Identity.AccessControl.List,
            FeaturePermissions.Admin.Identity.AccessControl.View,
            FeaturePermissions.Admin.Catalog.View,
            FeaturePermissions.Admin.Catalog.Create,
            FeaturePermissions.Admin.Catalog.Edit,
            FeaturePermissions.Admin.Catalog.Delete
        };
    }
}