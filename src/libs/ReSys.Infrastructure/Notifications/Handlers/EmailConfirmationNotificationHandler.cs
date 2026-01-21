using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Common;

namespace ReSys.Infrastructure.Notifications.Handlers;

public sealed class EmailConfirmationNotificationHandler(
    UserManager<User> userManager,
    IServiceProvider serviceProvider) : INotificationHandler<UserEvents.EmailConfirmationRequested>
{
    public async Task Handle(UserEvents.EmailConfirmationRequested notification, CancellationToken cancellationToken)
    {
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        await userManager.GenerateAndSendConfirmationEmailAsync(
            notificationService, 
            configuration, 
            notification.User, 
            null, 
            cancellationToken);
    }
}
