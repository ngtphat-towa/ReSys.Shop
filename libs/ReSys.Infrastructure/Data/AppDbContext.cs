using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Interfaces;

namespace ReSys.Infrastructure.Data;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductEmbedding> ProductEmbeddings => Set<ProductEmbedding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProductEmbedding>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.Embedding).HasColumnType("vector(384)");
            
            entity.HasOne(e => e.Product)
                .WithOne(p => p.Embedding)
                .HasForeignKey<ProductEmbedding>(e => e.ProductId);
        });
    }
}
