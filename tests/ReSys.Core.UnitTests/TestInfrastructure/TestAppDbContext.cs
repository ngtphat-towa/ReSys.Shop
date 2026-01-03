using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Infrastructure.Data;

namespace ReSys.Core.UnitTests.TestInfrastructure;

public class TestAppDbContext : AppDbContext
{
    public TestAppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore the Vector property for InMemory tests as it's not supported
        modelBuilder.Entity<ProductEmbedding>().Ignore(e => e.Embedding);
    }
}
