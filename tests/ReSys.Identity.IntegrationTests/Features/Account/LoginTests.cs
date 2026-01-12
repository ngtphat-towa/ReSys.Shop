using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Identity.Features.Account;
using ReSys.Identity.IntegrationTests.TestInfrastructure;
using Xunit;

namespace ReSys.Identity.IntegrationTests.Features.Account;

public class LoginTests(IdentityApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Post_Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid()}@test.com";
        var password = "Pass123$";
        
        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ReSys.Identity.Domain.ApplicationUser>>();
            await userManager.CreateAsync(new ReSys.Identity.Domain.ApplicationUser { UserName = email, Email = email }, password);
        }

        var request = new Login.Request(email, password, false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/login", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Should NOT have authentication cookies
        response.Headers.Should().NotContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Post_Login_WithInvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var request = new Login.Request("nonexistent@test.com", "WrongPass123!", false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/login", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Or whatever Validation returns
    }
}