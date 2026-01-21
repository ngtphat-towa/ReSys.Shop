using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Ordering;

namespace ReSys.Infrastructure.Backgrounds;

public class CartCleanupService(IServiceProvider serviceProvider, ILogger<CartCleanupService> logger) : BackgroundService
{
    private const int CartExpirationDays = 30;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Cart Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanAbandonedCartsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while cleaning abandoned carts.");
            }

            // Wait for next cycle
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanAbandonedCartsAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-CartExpirationDays);

        // Efficient bulk delete
        var deletedCount = await dbContext.Set<Order>()
            .Where(o => o.State == Order.OrderState.Cart && (o.UpdatedAt ?? o.CreatedAt) < cutoffDate)
            .ExecuteDeleteAsync(ct);

        if (deletedCount > 0)
        {
            logger.LogInformation("Cleaned up {Count} abandoned carts older than {Days} days.", deletedCount, CartExpirationDays);
        }
    }
}
