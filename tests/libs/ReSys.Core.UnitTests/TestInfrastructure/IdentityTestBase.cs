using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ReSys.Core.Common.Notifications.Interfaces;
using MediatR;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Infrastructure.Persistence.Interceptors;
using ReSys.Core.Common.Data;

namespace ReSys.Core.UnitTests.TestInfrastructure;

public abstract class IdentityTestBase(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture>
{
    protected readonly TestDatabaseFixture Fixture = fixture;

    protected (AppDbContext context, IServiceProvider serviceProvider) CreateTestContext()
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        services.AddLogging();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        var configuration = Substitute.For<IConfiguration>();
        services.AddSingleton(configuration);

        var notificationService = Substitute.For<INotificationService>();
        services.AddSingleton(notificationService);

        var mediator = Substitute.For<IMediator>();
        services.AddSingleton(mediator);

        var userContext = Substitute.For<IUserContext>();
        services.AddSingleton(userContext);

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<AppDbContext>((sp, o) =>
        {
            o.UseInMemoryDatabase(dbName);
            o.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        var sp = services.BuildServiceProvider();
        var context = sp.GetRequiredService<AppDbContext>();

        return (context, sp);
    }
}