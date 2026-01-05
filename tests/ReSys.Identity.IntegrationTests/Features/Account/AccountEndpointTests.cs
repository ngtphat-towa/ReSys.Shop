using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ReSys.Core.Domain.Identity;
using ReSys.Identity.Features.Account.Contracts;
using ReSys.Identity.IntegrationTests.Infrastructure;
using Xunit;

namespace ReSys.Identity.IntegrationTests.Features.Account;

public class AccountEndpointTests : IClassFixture<IdentityTestWebAppFactory>, IAsyncLifetime
{
    private readonly IdentityTestWebAppFactory _factory;
    private readonly HttpClient _client;

    public AccountEndpointTests(IdentityTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public ValueTask InitializeAsync() => new ValueTask(_factory.ResetDatabaseAsync());
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ForgotPassword_ShouldSendEmail_WhenUserExists()
    {
        // Arrange - Create user first
        var createUserRequest = new CreateUserRequest("forgot@test.com", "Password123!", "John", "Doe", UserType.Customer);
        await _client.PostAsJsonAsync("/api/users", createUserRequest, TestContext.Current.CancellationToken);

        // Act
        var request = new ForgotPasswordRequest("forgot@test.com");
        var response = await _client.PostAsJsonAsync("/api/account/forgot-password", request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _factory.EmailSender.LastToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResetPassword_ShouldSucceed_WithValidToken()
    {
        // Arrange
        var email = "reset@test.com";
        var createUserRequest = new CreateUserRequest(email, "OldPassword123!", "John", "Doe", UserType.Customer);
        await _client.PostAsJsonAsync("/api/users", createUserRequest, TestContext.Current.CancellationToken);

        // Request Token
        await _client.PostAsJsonAsync("/api/account/forgot-password", new ForgotPasswordRequest(email), TestContext.Current.CancellationToken);
        var token = _factory.EmailSender.LastToken;

        // Act
        var resetRequest = new ResetPasswordRequest(email, token, "NewPassword123!");
        var response = await _client.PostAsJsonAsync("/api/account/reset-password", resetRequest, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}