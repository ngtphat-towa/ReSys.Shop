using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NSubstitute;

using ReSys.Core.Common.Data;
using ReSys.Infrastructure.Persistence;

namespace ReSys.Core.UnitTests.TestInfrastructure;

public class TestDatabaseFixture : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    public IApplicationDbContext Context { get; }

    public TestDatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new TestAppDbContext(_options);
    }

    public AppDbContext CreateContext() => new TestAppDbContext(_options);

    public ILogger<T> CreateLogger<T>() => Substitute.For<ILogger<T>>();

    public void Dispose()
    {
        if (Context is DbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }
    }
}