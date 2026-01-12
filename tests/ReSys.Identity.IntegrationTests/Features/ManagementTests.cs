using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Constants;
using ReSys.Identity.Domain;
using ReSys.Identity.Features.Management.Permissions;
using ReSys.Identity.Features.Management.Roles;
using ReSys.Identity.Features.Management.Users;
using ReSys.Identity.IntegrationTests.TestInfrastructure;
using Xunit;

using PermissionCreate = ReSys.Identity.Features.Management.Permissions.Create;
using RoleCreate = ReSys.Identity.Features.Management.Roles.Create;
using ListRoles = ReSys.Identity.Features.Management.Roles.List;
using UpdateRolePermissions = ReSys.Identity.Features.Management.Roles.UpdatePermissions;
using UpdateUserPermissions = ReSys.Identity.Features.Management.Users.UpdatePermissions;

namespace ReSys.Identity.IntegrationTests.Features;

public class ManagementTests(IdentityApiFactory factory) : BaseIntegrationTest(factory)
{
    private async Task<string> AuthenticateAsync(bool asAdmin = false, string[]? permissions = null)
    {
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"user_{Guid.NewGuid()}@test.com";
        var user = new ApplicationUser { UserName = email, Email = email };
        await userManager.CreateAsync(user, "Pass123$");

        if (asAdmin)
        {
            var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            if (!await roleManager.RoleExistsAsync(AuthConstants.Roles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(AuthConstants.Roles.Admin));
            }
            await userManager.AddToRoleAsync(user, AuthConstants.Roles.Admin);
        }

        if (permissions != null)
        {
            foreach (var perm in permissions)
            {
                await userManager.AddClaimAsync(user, new Claim("permission", perm));
            }
        }

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", "Pass123$"),
            new KeyValuePair<string, string>("scope", "openid roles permissions") 
        });

        var response = await Client.PostAsync("/connect/token", content);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return result!.AccessToken!;
    }

    private class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    [Fact]
    public async Task CreatePermission_WithPermission_ReturnsOk()
    {
        // Arrange
        var token = await AuthenticateAsync(permissions: [AppPermissions.Identity.PermissionsManagement.Create]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new PermissionCreate.Request("System", "test.permission", "Test Permission", "Test");

        // Act
        var response = await Client.PostAsJsonAsync("/api/management/permissions", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePermission_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var token = await AuthenticateAsync(permissions: ["some.other.permission"]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new PermissionCreate.Request("System", "test.permission.2", "Test Permission 2", "Test");

        // Act
        var response = await Client.PostAsJsonAsync("/api/management/permissions", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateRolePermissions_WithPermission_UpdatesClaims()
    {
        // Arrange
        var token = await AuthenticateAsync(permissions: [AppPermissions.Identity.Roles.ManagePermissions]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create Role
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var role = new IdentityRole($"TestRole_{Guid.NewGuid()}");
        await roleManager.CreateAsync(role);

        var request = new UpdateRolePermissions.Request(new List<string> { "new.permission.1", "new.permission.2" });

        // Act
        var response = await Client.PutAsJsonAsync($"/api/management/roles/{role.Id}/permissions", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedRole = await roleManager.FindByIdAsync(role.Id);
        var claims = await roleManager.GetClaimsAsync(updatedRole!);
        claims.Should().Contain(c => c.Type == "permission" && c.Value == "new.permission.1");
        claims.Should().Contain(c => c.Type == "permission" && c.Value == "new.permission.2");
    }

    [Fact]
    public async Task UpdateUserDirectPermissions_WithPermission_UpdatesClaims()
    {
        // Arrange
        var token = await AuthenticateAsync(permissions: [AppPermissions.Identity.Users.ManagePermissions]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create Target User
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var targetUser = new ApplicationUser { UserName = $"target_{Guid.NewGuid()}", Email = $"target_{Guid.NewGuid()}@test.com" };
        await userManager.CreateAsync(targetUser, "Pass123$");

        var request = new UpdateUserPermissions.Request(new List<string> { "special.permission" });

        // Act
        var response = await Client.PutAsJsonAsync($"/api/management/users/{targetUser.Id}/permissions", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedUser = await userManager.FindByIdAsync(targetUser.Id);
        var claims = await userManager.GetClaimsAsync(updatedUser!);
        claims.Should().Contain(c => c.Type == "permission" && c.Value == "special.permission");
    }
}
