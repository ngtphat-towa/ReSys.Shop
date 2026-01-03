using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Interfaces;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerSettings JsonSettings;
    protected readonly IServiceScope Scope;
    protected readonly IApplicationDbContext Context;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        JsonSettings = factory.JsonSettings;
        Scope = factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        Scope.Dispose();
        return Task.CompletedTask;
    }
}