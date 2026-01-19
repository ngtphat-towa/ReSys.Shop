using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ReSys.AppHost.Tests.Fixtures;

/// <summary>
/// Shared fixture that starts the AppHost once for all tests in a class.
/// Dramatically improves test performance by avoiding repeated startup.
/// </summary>
public class AppHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);
    
    public DistributedApplication App { get; set; } = null!;
    public ResourceNotificationService ResourceNotificationService { get; set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Environment.SetEnvironmentVariable("SKIP_FRONTEND", "true");
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.ReSys_AppHost>();

        // Detailed logging for debugging
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter("Aspire", LogLevel.Debug);
            logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Debug);
        });

        // Add resilience to HTTP clients
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await appHost.BuildAsync();
        ResourceNotificationService = App.Services.GetRequiredService<ResourceNotificationService>();

        await App.StartAsync();

        // Wait for critical resources in parallel
        await Task.WhenAll(
            WaitForResourceAsync("postgres"),
            WaitForResourceAsync("ml")
        );

        await Task.WhenAll(
            WaitForResourceAsync("api"),
            WaitForResourceAsync("gateway")
        );
    }

    public async Task WaitForResourceAsync(string resourceName, TimeSpan? timeout = null)
    {
        await ResourceNotificationService
            .WaitForResourceAsync(resourceName, KnownResourceStates.Running, TestContext.Current.CancellationToken)
            .WaitAsync(timeout ?? DefaultTimeout, TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (App is not null)
        {
            await App.DisposeAsync();
        }
    }
}
