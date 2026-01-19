using Microsoft.AspNetCore.Identity;
using ReSys.Core.Common.Security.Authentication.Externals.Models;
using ReSys.Core.Domain.Identity.Users;
using ErrorOr;

namespace ReSys.Core.Common.Security.Authentication.Externals.Interfaces;

public interface IExternalUserService
{
    Task<ErrorOr<(User User, bool IsNewUser, bool IsNewLogin)>> FindOrCreateUserWithExternalLoginAsync(
        ExternalUserTransfer externalUserTransfer,
        string provider,
        CancellationToken cancellationToken = default);

    Task<bool> HasExternalLoginAsync(string userId, string provider, CancellationToken cancellationToken = default);
    Task<IList<UserLoginInfo>> GetExternalLoginsAsync(string userId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> RemoveExternalLoginAsync(
        string userId,
        string provider,
        string providerKey,
        CancellationToken cancellationToken = default);
}
