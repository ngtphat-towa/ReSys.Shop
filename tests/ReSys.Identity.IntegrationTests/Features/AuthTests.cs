using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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
        var response = await Client.PostAsync("/connect/token", content, CancellationToken.None);

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
        var response = await Client.PostAsync("/connect/token", content, CancellationToken.None);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
             var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
             throw new Exception($"Request failed with {response.StatusCode}: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(CancellationToken.None);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    private class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}