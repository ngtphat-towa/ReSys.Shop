using MediatR;

using Microsoft.AspNetCore.Identity;

using Mapster;

using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Internal.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Internal.Sessions;

public static class GetSession
{
    // Response: Merged profile + permissions
    public record Response : UserProfileResponse
    {
        public List<string> Roles { get; init; } = [];
        public List<string> Permissions { get; init; } = [];
    }

    public record Query : IRequest<ErrorOr<Response>>;

    public class Handler(
        IUserContext userContext,
        UserManager<User> userManager,
        IAuthorizeClaimDataProvider authDataProvider) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return UserErrors.Unauthorized;

            // 1. Fetch user profile from DB
            var user = await userManager.FindByIdAsync(userContext.UserId);
            if (user == null) return UserErrors.NotFound(userContext.UserId);

            // 2. Fetch permissions from Cache/Provider
            var authData = await authDataProvider.GetUserAuthorizationAsync(user.Id);

            // 3. Map result
            var response = user.Adapt<Response>() with
            {
                Roles = authData?.Roles.ToList() ?? [],
                Permissions = authData?.Permissions.ToList() ?? []
            };

            return response;
        }
    }
}