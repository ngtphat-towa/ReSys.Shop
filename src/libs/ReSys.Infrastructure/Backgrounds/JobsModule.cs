using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using ReSys.Infrastructure.Backgrounds.Jobs;
using ReSys.Shared.Constants;
using ReSys.Shared.Telemetry;

namespace ReSys.Infrastructure.Backgrounds;

public static class JobsModule
{
    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterModule(ModuleNames.Infrastructure, "Jobs", s =>
        {
            s.AddQuartz(q =>
            {
                var jobKey = new JobKey("CartCleanup");
                q.AddJob<CartCleanupJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("CartCleanup-Trigger")
                    .WithCronSchedule("0 0 2 * * ?")); // Run daily at 2 AM
            });

            s.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        });

        return services;
    }
}
