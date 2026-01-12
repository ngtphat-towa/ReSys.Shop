using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReSys.Identity.IntegrationTests.TestInfrastructure;
using Xunit;
using ReSys.Identity.Features.Account;

namespace ReSys.Identity.IntegrationTests.Features;

public class RegisterTests(IdentityApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new Register.Request("newuser@shop.com", "Pass123$", "Pass123$");

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/register", request, CancellationToken.None);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
             var body = await response.Content.ReadAsStringAsync(CancellationToken.None);
             throw new Exception($"Request failed with {response.StatusCode}: {body}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK); 
    }

    [Fact]
    public async Task Register_WithMismatchPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new Register.Request("baduser@shop.com", "Pass123$", "Mismatch");

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/register", request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}