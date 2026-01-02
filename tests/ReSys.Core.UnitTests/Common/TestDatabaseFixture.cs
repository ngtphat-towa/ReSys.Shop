using Microsoft.EntityFrameworkCore;
using ReSys.Core.Interfaces;
using ReSys.Infrastructure.Data;

namespace ReSys.Core.UnitTests.Common;

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

    public void Dispose()
    {
        if (Context is DbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
        }
    }
}
