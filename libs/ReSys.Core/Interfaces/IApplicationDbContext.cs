using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;

namespace ReSys.Core.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductEmbedding> ProductEmbeddings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
