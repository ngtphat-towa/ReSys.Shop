using Microsoft.EntityFrameworkCore;

using ReSys.Core.Domain.Testing.Examples;
using ReSys.Infrastructure.Persistence;

namespace ReSys.Core.UnitTests.TestInfrastructure;

public class TestAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore the Vector property for InMemory tests as it's not supported
        modelBuilder.Entity<ExampleEmbedding>().Ignore(e => e.Embedding);
    }
}
