using ReSys.AppHost.Tests.Fixtures;

namespace ReSys.AppHost.Tests.Integration;

[Trait("Category", "Integration")]
public class MlServiceIntegrationTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    [Fact(DisplayName = "ML service health endpoint should return 200 OK")]
    public async Task MlHealth_Should_ReturnOk()
    {
        // Arrange
        var httpClient = fixture.App.CreateHttpClient("ml");

        // Act
        var response = await httpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "ML service should have mock mode enabled")]
    public async Task MlService_Should_HaveMockModeEnabled()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var ml = model.Resources.OfType<IResourceWithEnvironment>()
            .Single(r => r.Name == "ml");

#pragma warning disable CS0618 // Type or member is obsolete
        // Act
        var envVars = await ml.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        envVars.Should().ContainKey("USE_MOCK_ML");
        envVars["USE_MOCK_ML"].Should().Be("true");
    }

    [Fact(DisplayName = "ML service should use correct root path")]
    public async Task MlService_Should_UseCorrectRootPath()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var ml = model.Resources.OfType<IResourceWithEnvironment>()
            .Single(r => r.Name == "ml");

#pragma warning disable CS0618 // Type or member is obsolete
        // Act
        var envVars = await ml.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        envVars.Should().ContainKey("ROOT_PATH");
        envVars["ROOT_PATH"].Should().Be("/ml");
    }
}
