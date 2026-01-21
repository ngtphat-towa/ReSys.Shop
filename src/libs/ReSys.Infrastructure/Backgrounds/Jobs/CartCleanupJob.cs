using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;

namespace ReSys.Infrastructure.Backgrounds.Jobs;

[DisallowConcurrentExecution]
public class CartCleanupJob(IApplicationDbContext dbContext, ILogger<CartCleanupJob> logger) : IJob
{
    private const int CartExpirationDays = 30;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting Cart Cleanup Job...");

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-CartExpirationDays);

        var deletedCount = await dbContext.Set<Order>()
            .Where(o => o.State == Order.OrderState.Cart && (o.UpdatedAt ?? o.CreatedAt) < cutoffDate)
            .ExecuteDeleteAsync(context.CancellationToken);

        if (deletedCount > 0)
        {
            logger.LogInformation("Cleaned up {Count} abandoned carts older than {Days} days.", deletedCount, CartExpirationDays);
        }
        
        logger.LogInformation("Cart Cleanup Job completed.");
    }
}
