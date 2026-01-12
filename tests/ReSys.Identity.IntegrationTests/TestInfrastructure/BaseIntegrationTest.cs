using Microsoft.Extensions.DependencyInjection;
using ReSys.Identity.Persistence;
using Xunit;

namespace ReSys.Identity.IntegrationTests.TestInfrastructure;

[Collection("Shared Identity Database")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IdentityApiFactory Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly AppIdentityDbContext Context;

    protected BaseIntegrationTest(IdentityApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    }

    public virtual async ValueTask InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        Scope.Dispose();
        return ValueTask.CompletedTask;
    }
}