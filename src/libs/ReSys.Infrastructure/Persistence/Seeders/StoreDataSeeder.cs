using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.Stores;

namespace ReSys.Infrastructure.Persistence.Seeders;

public class StoreDataSeeder(IApplicationDbContext dbContext, ILogger<StoreDataSeeder> logger) : IDataSeeder
{
    public int Order => 1; 

    public async Task<ErrorOr<Success>> SeedAsync(CancellationToken cancellationToken)
    {
        if (!await dbContext.Set<Store>().AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seeding default store...");
            
            var storeResult = Store.Create("Main Store", "MAIN", "USD");
            if (storeResult.IsError) return storeResult.Errors;
            
            var store = storeResult.Value;
            
            dbContext.Set<Store>().Add(store);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        
        return Result.Success;
    }
}