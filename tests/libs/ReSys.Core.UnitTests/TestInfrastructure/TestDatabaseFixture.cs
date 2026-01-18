using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReSys.Core.Common.Data;
using ReSys.Infrastructure.Persistence;

namespace ReSys.Core.UnitTests.TestInfrastructure;

public class TestDatabaseFixture : IDisposable
{
    public IApplicationDbContext Context { get; }

    public TestDatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new TestAppDbContext(options);
    }

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
