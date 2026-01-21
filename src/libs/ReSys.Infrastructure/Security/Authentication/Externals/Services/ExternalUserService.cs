using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Security.Authentication.External;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Infrastructure.Security.Authentication.Externals.Services;

public sealed class ExternalUserService(
    UserManager<User> userManager) : IExternalUserService
{
    public async Task<ErrorOr<(User User, bool IsNewUser, bool IsNewLogin)>> FindOrCreateUserWithExternalLoginAsync(
        ExternalUserTransfer info, string provider, CancellationToken ct = default)
    {
        var existingUser = await userManager.FindByLoginAsync(provider, info.ProviderId);
        if (existingUser != null) return (existingUser, false, false);

        var userByEmail = await userManager.FindByEmailAsync(info.Email);
        if (userByEmail != null)
        {
            var result = await userManager.AddLoginAsync(userByEmail, new UserLoginInfo(provider, info.ProviderId, provider));
            return result.Succeeded ? (userByEmail, false, true) : Error.Failure("ExternalLogin.LinkFailed");
        }

        var newUser = User.Create(info.Email, firstName: info.FirstName, lastName: info.LastName, emailConfirmed: info.EmailVerified).Value;
        var createResult = await userManager.CreateAsync(newUser);
        if (!createResult.Succeeded) return Error.Failure("User.CreationFailed");

        await userManager.AddLoginAsync(newUser, new UserLoginInfo(provider, info.ProviderId, provider));
        return (newUser, true, true);
    }

    public async Task<bool> HasExternalLoginAsync(string userId, string provider, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return false;
        var logins = await userManager.GetLoginsAsync(user);
        return logins.Any(l => l.LoginProvider == provider);
    }

    public async Task<IList<UserLoginInfo>> GetExternalLoginsAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user != null ? await userManager.GetLoginsAsync(user) : new List<UserLoginInfo>();
    }

    public async Task<ErrorOr<Success>> RemoveExternalLoginAsync(string userId, string provider, string providerKey, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Error.NotFound();
        var result = await userManager.RemoveLoginAsync(user, provider, providerKey);
        return result.Succeeded ? Result.Success : Error.Failure("ExternalLogin.RemovalFailed");
    }
}
