using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Common;

namespace ReSys.Infrastructure.Notifications.Handlers;

public sealed class UserCreatedNotificationHandler(
    UserManager<User> userManager,
    IServiceProvider serviceProvider) : INotificationHandler<UserEvents.UserCreated>
{
    public async Task Handle(UserEvents.UserCreated notification, CancellationToken cancellationToken)
    {
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // For new users, we want to send the initial email confirmation
        await userManager.GenerateAndSendConfirmationEmailAsync(
            notificationService, 
            configuration, 
            notification.User, 
            null, 
            cancellationToken);
    }
}
