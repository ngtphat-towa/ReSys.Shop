using System.Net.Http.Json;
using FluentAssertions;
using ReSys.Core.Domain.Identity;
using ReSys.Identity.Features.Account.Contracts;
using ReSys.Identity.IntegrationTests.Infrastructure;
using Xunit;

namespace ReSys.Identity.IntegrationTests.Features.Account;

public class UsersEndpointTests : IClassFixture<IdentityTestWebAppFactory>, IAsyncLifetime
{
    private readonly IdentityTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public UsersEndpointTests(IdentityTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public ValueTask InitializeAsync() => new ValueTask(_factory.ResetDatabaseAsync());
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task CreateUser_ShouldCreateUser_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateUserRequest("test@test.com", "Password123!", "John", "Doe", UserType.Customer);

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var location = response.Headers.Location;
        location.Should().NotBeNull();

        // Verify creation by GET
        var getResponse = await _client.GetAsync(location, TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var user = await getResponse.Content.ReadFromJsonAsync<UserResponse>(TestContext.Current.CancellationToken);
        user.Should().NotBeNull();
        user!.Email.Should().Be(request.Email);
        user.FirstName.Should().Be(request.FirstName);
    }
}