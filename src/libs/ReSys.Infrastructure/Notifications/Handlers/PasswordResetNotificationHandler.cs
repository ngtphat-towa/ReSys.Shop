using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Common;

namespace ReSys.Infrastructure.Notifications.Handlers;

public sealed class PasswordResetNotificationHandler(
    UserManager<User> userManager,
    IServiceProvider serviceProvider) : INotificationHandler<UserEvents.PasswordResetRequested>
{
    public async Task Handle(UserEvents.PasswordResetRequested notification, CancellationToken cancellationToken)
    {
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        await userManager.GenerateAndSendPasswordResetCodeAsync(
            notificationService, 
            configuration, 
            notification.User, 
            cancellationToken);
    }
}
