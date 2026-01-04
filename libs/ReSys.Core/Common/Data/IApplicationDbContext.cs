using Microsoft.EntityFrameworkCore;

namespace ReSys.Core.Common.Data;

public interface IApplicationDbContext
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
