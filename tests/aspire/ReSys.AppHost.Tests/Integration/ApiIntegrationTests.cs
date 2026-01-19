using ReSys.AppHost.Tests.Fixtures;

namespace ReSys.AppHost.Tests.Integration;

[Trait("Category", "Integration")]
public class ApiIntegrationTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    [Fact(DisplayName = "API health endpoint should return 200 OK")]
    public async Task ApiHealth_Should_ReturnOk()
    {
        // Arrange
        var httpClient = fixture.App.CreateHttpClient("api");

        // Act
        var response = await httpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "API should handle concurrent requests")]
    public async Task Api_Should_HandleConcurrentRequests()
    {
        // Arrange
        var httpClient = fixture.App.CreateHttpClient("api");
        var concurrentRequests = 10;

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => httpClient.GetAsync("/health", TestContext.Current.CancellationToken));
        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(concurrentRequests);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }

    [Fact(DisplayName = "API should reference database in environment")]
    public async Task Api_Should_ReferenceDatabaseInEnvironment()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var api = model.Resources.OfType<ProjectResource>()
            .Single(r => r.Name == "api");

#pragma warning disable CS0618 // Type or member is obsolete
        // Act
        var envVars = await api.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        envVars.Keys.Should().Contain(key => key.Contains("ConnectionStrings"));
    }

    [Fact(DisplayName = "API should reference ML service in environment")]
    public async Task Api_Should_ReferenceMlServiceInEnvironment()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var api = model.Resources.OfType<ProjectResource>()
            .Single(r => r.Name == "api");

#pragma warning disable CS0618 // Type or member is obsolete
        // Act
        var envVars = await api.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        envVars.Keys.Should().Contain(key => key.Contains("MlOptions__ServiceUrl"));
    }
}
