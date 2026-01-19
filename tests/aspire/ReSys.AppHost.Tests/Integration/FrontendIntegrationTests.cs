using ReSys.AppHost.Tests.Fixtures;

namespace ReSys.AppHost.Tests.Integration;

[Trait("Category", "Integration")]
public class FrontendIntegrationTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    private static readonly TimeSpan FrontendTimeout = TimeSpan.FromSeconds(90);

    [Fact(DisplayName = "Shop frontend should be accessible")]
    public async Task ShopFrontend_Should_BeAccessible()
    {
        // Arrange - Frontends take longer to start
        await fixture.WaitForResourceAsync("shop", FrontendTimeout);
        var httpClient = fixture.App.CreateHttpClient("shop");

        // Act
        var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Shop frontend should serve HTML")]
    public async Task ShopFrontend_Should_ServeHtml()
    {
        // Arrange
        await fixture.WaitForResourceAsync("shop", FrontendTimeout);
        var httpClient = fixture.App.CreateHttpClient("shop");

        // Act
        var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Contain("<!DOCTYPE html>");
    }

    [Fact(DisplayName = "Admin frontend should be accessible")]
    public async Task AdminFrontend_Should_BeAccessible()
    {
        // Arrange
        await fixture.WaitForResourceAsync("admin", FrontendTimeout);
        var httpClient = fixture.App.CreateHttpClient("admin");

        // Act
        var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Shop should reference API in environment")]
    public async Task Shop_Should_ReferenceApi()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var shop = model.Resources.OfType<IResourceWithEnvironment>()
            .Single(r => r.Name == "shop");

#pragma warning disable CS0618 // Type or member is obsolete
        // Act
        var envVars = await shop.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        envVars.Keys.Should().Contain(key => key.Contains("services__api__"));
    }

    [Fact(DisplayName = "Admin should reference API in environment")]
    public async Task Admin_Should_ReferenceApi()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var admin = model.Resources.OfType<IResourceWithEnvironment>()
            .Single(r => r.Name == "admin");

#pragma warning disable CS0618 // Type or member is obsolete
        // Act
        var envVars = await admin.GetEnvironmentVariableValuesAsync(DistributedApplicationOperation.Run);
#pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        envVars.Keys.Should().Contain(key => key.Contains("services__api__"));
    }
}
