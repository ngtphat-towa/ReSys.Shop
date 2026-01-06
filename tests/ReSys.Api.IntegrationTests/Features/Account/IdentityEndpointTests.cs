using System.Net;
using System.Net.Http.Json;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;
using ReSys.Core.Common.Models;
using ReSys.Api.IntegrationTests.TestInfrastructure;

namespace ReSys.Api.IntegrationTests.Features.Account;

[Collection("Shared Database")]
public class IdentityEndpointTests : BaseIntegrationTest
{
    public IdentityEndpointTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    #region User Tests
    [Fact]
    public async Task CreateUser_ShouldCreateUser_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateUserRequest("test@test.com", "Password123!", "John", "Doe", UserType.Customer);

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", request, Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var location = response.Headers.Location;
        location.Should().NotBeNull();

        // Verify creation by GET
        var getResponse = await Client.GetAsync(location, TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>(Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);
        apiResponse.Should().NotBeNull();
        var user = apiResponse!.Data;
        user.Should().NotBeNull();
        user!.Email.Should().Be(request.Email);
        user.FirstName.Should().Be(request.FirstName);
    }
    #endregion

    #region Role Tests
    [Fact]
    public async Task CreateRole_ShouldSucceed_WhenValid()
    {
        var request = new CreateRoleRequest("Manager");
        var response = await Client.PostAsJsonAsync("/api/roles", request, Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task DeleteRole_ShouldSucceed_WhenExists()
    {
        // Arrange
        var createResponse = await Client.PostAsJsonAsync("/api/roles", new CreateRoleRequest("ToDelete"), Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);
        var location = createResponse.Headers.Location;
        location.Should().NotBeNull();

        // Act
        var response = await Client.DeleteAsync(location!, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await Client.GetAsync(location!, TestContext.Current.CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    #endregion

    #region Account Tests
    [Fact]
    public async Task ForgotPassword_ShouldSendEmail_WhenUserExists()
    {
        // Arrange - Create user first
        var email = "forgot@test.com";
        var createUserRequest = new CreateUserRequest(email, "Password123!", "John", "Doe", UserType.Customer);
        await Client.PostAsJsonAsync("/api/users", createUserRequest, Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);

        // Act
        var request = new ForgotPasswordRequest(email);
        var response = await Client.PostAsJsonAsync("/api/account/forgot-password", request, Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            throw new Exception($"ForgotPassword failed with {response.StatusCode}. Body: {content}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        Factory.EmailSender.LastToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResetPassword_ShouldSucceed_WithValidToken()
    {
        // Arrange
        var email = "reset@test.com";
        var createUserRequest = new CreateUserRequest(email, "OldPassword123!", "John", "Doe", UserType.Customer);
        await Client.PostAsJsonAsync("/api/users", createUserRequest, Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);

        // Request Token
        await Client.PostAsJsonAsync("/api/account/forgot-password", new ForgotPasswordRequest(email), Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);
        var token = Factory.EmailSender.LastToken;
        token.Should().NotBeNullOrEmpty();

        // Act
        var resetRequest = new ResetPasswordRequest(email, token!, "NewPassword123!");
        var response = await Client.PostAsJsonAsync("/api/account/reset-password", resetRequest, Factory.DefaultJsonOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    #endregion
}
