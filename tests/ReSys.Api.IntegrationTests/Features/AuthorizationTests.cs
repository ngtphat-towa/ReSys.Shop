using System.Net;
using System.Net.Http.Json;


using ReSys.Api.IntegrationTests.TestInfrastructure;

namespace ReSys.Api.IntegrationTests.Features;

[Collection("Shared Database")]
public class AuthorizationTests(IntegrationTestWebAppFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ProtectedEndpoints_ReturnUnauthorized_WhenNoTokenProvided()
    {
        // Arrange
        var payload = new { name = "Unauthorized Category" };

        // Act & Assert
        
        // POST is protected
        var postResponse = await _client.PostAsJsonAsync("/api/testing/example-categories", payload, TestContext.Current.CancellationToken);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // PUT is protected
        var putResponse = await _client.PutAsJsonAsync($"/api/testing/example-categories/{Guid.NewGuid()}", payload, TestContext.Current.CancellationToken);
        putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // DELETE is protected
        var deleteResponse = await _client.DeleteAsync($"/api/testing/example-categories/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoints_ReturnForbidden_WhenCustomerTokenProvided()
    {
        // Arrange
        var customerClient = factory.CreateClient();
        customerClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token-customer");
        var payload = new { name = "Restricted Category" };

        // Act & Assert
        
        // POST is restricted to those with 'testing.example_categories.create' permission.
        var postResponse = await customerClient.PostAsJsonAsync("/api/testing/example-categories", payload, TestContext.Current.CancellationToken);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ProtectedEndpoints_ReturnOk_WhenTokenWithPermissionProvided()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token-with-perm");
        var payload = new { name = "Permitted Category" };

        // Act
        var response = await client.PostAsJsonAsync("/api/testing/example-categories", payload, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PublicEndpoints_ReturnOk_WhenNoTokenProvided()
    {
        // Act
        var response = await _client.GetAsync("/api/testing/example-categories", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
