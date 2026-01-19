using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ErrorOr;

namespace ReSys.Infrastructure.Persistence.Seeders;

/// <summary>
/// Orchestrates the execution of all registered data seeders in their defined order.
/// </summary>
public sealed class SeederOrchestrator(
    IServiceProvider serviceProvider,
    ILogger<SeederOrchestrator> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting database seeding orchestration...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();

            foreach (var seeder in seeders.OrderBy(s => s.Order))
            {
                var seederName = seeder.GetType().Name;
                logger.LogInformation("Running seeder: {SeederName}", seederName);
                
                var result = await seeder.SeedAsync(cancellationToken);

                if (result.IsError)
                {
                    logger.LogError("Seeder {SeederName} failed: {Errors}", 
                        seederName, 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogInformation("Seeder {SeederName} completed successfully.", seederName);
                }
            }

            logger.LogInformation("Database seeding orchestration finished.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "A fatal error occurred during database seeding orchestration.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}