using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Constants;
using ReSys.Identity.Domain;
using ReSys.Identity.IntegrationTests.TestInfrastructure;
using Xunit;

namespace ReSys.Identity.IntegrationTests.Features;

public class AuthTests(IdentityApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetToken_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", "unknown"),
            new KeyValuePair<string, string>("password", "badpass"),
            new KeyValuePair<string, string>("scope", "offline_access")
        });

        // Act
        var response = await Client.PostAsync("/connect/token", content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetToken_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "testuser", Email = "test@test.com" };
        await userManager.CreateAsync(user, "Pass123$");

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("username", "testuser"),
            new KeyValuePair<string, string>("password", "Pass123$"),
            new KeyValuePair<string, string>("scope", "offline_access")
        });

        // Act
        var response = await Client.PostAsync("/connect/token", content, TestContext.Current.CancellationToken);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
             var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
             throw new Exception($"Request failed with {response.StatusCode}: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetToken_UserWithRole_ReturnsTokenWithRoleClaim()
    {
        // Arrange
        var userManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        var roleName = AuthConstants.Roles.Admin;
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var email = $"roleuser_{Guid.NewGuid()}@test.com";
        var user = new ApplicationUser { UserName = email, Email = email };
        await userManager.CreateAsync(user, "Pass123$");
        await userManager.AddToRoleAsync(user, roleName);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", "test-client"),
            new KeyValuePair<string, string>("username", email),
            new KeyValuePair<string, string>("password", "Pass123$"),
            new KeyValuePair<string, string>("scope", "openid roles")
        });

        // Act
        var response = await Client.PostAsync("/connect/token", content, TestContext.Current.CancellationToken);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"Request failed with {response.StatusCode}: {body}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken);
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result!.AccessToken);
        
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == roleName);
    }

    [Fact]
    public async Task DiscoveryEndpoint_ReturnsCorrectConfiguration()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/openid-configuration", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("connect/token");
        content.Should().Contain("jwks_uri");
    }

    [Fact]
    public async Task JwksEndpoint_ReturnsValidKeys()
    {
        // Arrange
        var discoveryResponse = await Client.GetAsync("/.well-known/openid-configuration", TestContext.Current.CancellationToken);
        var discovery = await discoveryResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(TestContext.Current.CancellationToken);
        var jwksUri = discovery.GetProperty("jwks_uri").GetString();

        // Act
        var response = await Client.GetAsync(jwksUri!, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        content.Should().Contain("keys");
    }

    private class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}
