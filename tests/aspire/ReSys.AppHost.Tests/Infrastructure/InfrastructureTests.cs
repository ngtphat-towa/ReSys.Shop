using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using ReSys.AppHost.Tests.Fixtures;

namespace ReSys.AppHost.Tests.Infrastructure;

[Trait("Category", "Infrastructure")]
public class InfrastructureTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    [Fact(DisplayName = "All core resources should reach Running state")]
    public async Task AllCoreResources_Should_ReachRunningState()
    {
        // Arrange
        var coreResources = new[] { "postgres", "api", "ml", "gateway" };

        // Act & Assert - Wait in parallel
        await Task.WhenAll(coreResources.Select(resource =>
            fixture.WaitForResourceAsync(resource, TimeSpan.FromSeconds(30))
        ));
    }

    [Fact(DisplayName = "Postgres should provide valid connection string")]
    public async Task Postgres_Should_ProvideValidConnectionString()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var postgres = model.Resources.OfType<PostgresServerResource>()
            .Single(r => r.Name == "postgres");

        // Act
        var connectionString = await postgres.GetConnectionStringAsync(TestContext.Current.CancellationToken);

        // Assert
        connectionString.Should().NotBeNullOrWhiteSpace();
        connectionString.Should().Contain("Host=");
        connectionString.Should().Contain("Database="); 
    }

    [Fact(DisplayName = "Postgres container should be persistent")]
    public void PostgresContainer_Should_BePersistent()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var postgres = model.Resources.OfType<PostgresServerResource>()
            .Single(r => r.Name == "postgres");

        // Act
        var lifetimeAnnotation = postgres.Annotations
            .OfType<ContainerLifetimeAnnotation>()
            .FirstOrDefault();

        // Assert
        lifetimeAnnotation.Should().NotBeNull();
        lifetimeAnnotation!.Lifetime.Should().Be(ContainerLifetime.Persistent);
    }

    [Fact(DisplayName = "Mail container should be persistent")]
    public void MailContainer_Should_BePersistent()
    {
        // Arrange
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var mail = model.Resources.OfType<ContainerResource>()
            .Single(r => r.Name == "mail");

        // Act
        var lifetimeAnnotation = mail.Annotations
            .OfType<ContainerLifetimeAnnotation>()
            .FirstOrDefault();

        // Assert
        lifetimeAnnotation.Should().NotBeNull();
        lifetimeAnnotation!.Lifetime.Should().Be(ContainerLifetime.Persistent);
    }
}
