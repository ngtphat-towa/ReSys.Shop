using Microsoft.Extensions.DependencyInjection;


using Newtonsoft.Json;


using ReSys.Core.Common.Data;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly HttpClient Client;
    protected readonly JsonSerializerSettings JsonSettings;
    protected readonly IServiceScope Scope;
    protected readonly IApplicationDbContext Context;
    protected readonly ITestOutputHelper Output;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory, ITestOutputHelper output)
    {
        Factory = factory;
        Output = output;

        Client = factory.CreateClient();
        JsonSettings = factory.JsonSettings;
        Scope = factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
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