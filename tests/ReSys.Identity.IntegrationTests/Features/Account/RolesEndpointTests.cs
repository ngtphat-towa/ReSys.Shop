using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReSys.Identity.Features.Account.Contracts;
using ReSys.Identity.IntegrationTests.Infrastructure;
using Xunit;

namespace ReSys.Identity.IntegrationTests.Features.Account;

public class RolesEndpointTests : IClassFixture<IdentityTestWebAppFactory>, IAsyncLifetime
{
    private readonly IdentityTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public RolesEndpointTests(IdentityTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public ValueTask InitializeAsync() => new ValueTask(_factory.ResetDatabaseAsync());
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task CreateRole_ShouldSucceed_WhenValid()
    {
        var request = new CreateRoleRequest("Manager");
        var response = await _client.PostAsJsonAsync("/api/roles", request, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task DeleteRole_ShouldSucceed_WhenExists()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/roles", new CreateRoleRequest("ToDelete"), TestContext.Current.CancellationToken);
        var location = createResponse.Headers.Location;
        location.Should().NotBeNull();

        // Act
        var response = await _client.DeleteAsync(location!, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await _client.GetAsync(location!, TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}