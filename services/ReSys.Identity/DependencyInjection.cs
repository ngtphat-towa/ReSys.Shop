using FluentValidation;
using Microsoft.Extensions.Hosting;
using ReSys.Core.Common.Behaviors;
using ReSys.Identity.Authentication;
using ReSys.Identity.Options;
using ReSys.Identity.Persistence;
using ReSys.Identity.Presentation;
using ReSys.Identity.Seeding;
using ReSys.Infrastructure.Notifications;

namespace ReSys.Identity;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddIdentityService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<IdentitySettings>()
            .Bind(builder.Configuration.GetSection(IdentitySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.AddPersistence();
        
        builder.Services
            .AddAuth()
            .AddPresentation()
            .AddNotifications(builder.Configuration);

        builder.Services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        builder.Services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        builder.Services.AddHostedService<IdentitySeeder>();

        return builder;
    }
}
