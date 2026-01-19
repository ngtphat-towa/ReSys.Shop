using ReSys.AppHost.Tests.Fixtures;

namespace ReSys.AppHost.Tests.Integration;

[Trait("Category", "Integration")]
public class GatewayIntegrationTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    [Fact(DisplayName = "Gateway health endpoint should return 200 OK")]
    public async Task GatewayHealth_Should_ReturnOk()
    {
        // Arrange
        var httpClient = fixture.App.CreateHttpClient("gateway");

        // Act
        var response = await httpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Gateway should reference all services in environment")]
    public async Task Gateway_Should_ReferenceAllServices()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var gateway = model.Resources.OfType<ProjectResource>()
            .Single(r => r.Name == "gateway");

#pragma warning disable CS0618 // Type or member is obsolete
        // Act
        var envVars = await gateway.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        envVars.Keys.Should().Contain(key => key.Contains("services__api__"));
        envVars.Keys.Should().Contain(key => key.Contains("services__ml__"));
        envVars.Keys.Should().Contain(key => key.Contains("services__shop__"));
        envVars.Keys.Should().Contain(key => key.Contains("services__admin__"));
    }
}