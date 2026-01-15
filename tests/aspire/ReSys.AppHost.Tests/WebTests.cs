using System.Net;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;

namespace ReSys.AppHost.Tests;

[Trait("Category", "System")]
[Trait("Module", "AppHost")]
public class WebTests
{
    [Fact(DisplayName = "All core resources should reach 'Running' state upon startup")]
    public async Task AllResources_Should_StartSuccessfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ReSys_AppHost>(TestContext.Current.CancellationToken);
        await using var app = await appHost.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);

        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();

        // Act & Assert
        // We wait for all core resources to reach the Running state
        string[] resources = ["postgres", "api", "gateway", "shop", "admin", "ml"];
        
        foreach (var resource in resources)
        {
            var waitTask = resourceNotificationService.WaitForResourceAsync(resource, KnownResourceStates.Running, TestContext.Current.CancellationToken)
                .WaitAsync(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken);
            
            await waitTask;
        }
    }

    [Fact(DisplayName = "API health check endpoint should return 200 OK")]
    public async Task ApiHealthCheck_Should_ReturnOk()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ReSys_AppHost>(TestContext.Current.CancellationToken);
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Gateway should correctly proxy requests to the API health endpoint")]
    public async Task Gateway_Should_RouteToApiCorrectly()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ReSys_AppHost>(TestContext.Current.CancellationToken);
        await using var app = await appHost.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);

        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceAsync("gateway", KnownResourceStates.Running, TestContext.Current.CancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("gateway");
        var response = await httpClient.GetAsync("/health", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Shop frontend root should be accessible and return 200 OK")]
    public async Task ShopFrontend_Should_ReturnOk()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ReSys_AppHost>(TestContext.Current.CancellationToken);
        await using var app = await appHost.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("shop");
        var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Postgres resource should provide a valid connection string")]
    public async Task Database_Should_ProvideValidConnectionString()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ReSys_AppHost>(TestContext.Current.CancellationToken);
        await using var app = await appHost.BuildAsync(TestContext.Current.CancellationToken);
        await app.StartAsync(TestContext.Current.CancellationToken);

        // Act
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var postgres = model.Resources.OfType<PostgresServerResource>().Single(r => r.Name == "postgres");
        var connectionString = await postgres.GetConnectionStringAsync(TestContext.Current.CancellationToken);

        // Assert
        connectionString.Should().NotBeNull();
        connectionString.Should().Contain("Host=");
    }
}
