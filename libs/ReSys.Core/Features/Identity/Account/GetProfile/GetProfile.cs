using System.Security.Claims;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Common;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Core.Features.Identity.Account.GetProfile;

public static class GetProfile
{
    public record Query(ClaimsPrincipal User) : IRequest<ErrorOr<UserResponse>>;

    public class Handler : IRequestHandler<Query, ErrorOr<UserResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<UserResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(request.User);
            if (user == null) return IdentityErrors.UserNotFound;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponse(
                user.Id,
                user.UserName ?? "",
                user.Email ?? "",
                user.FirstName ?? "",
                user.LastName ?? "",
                user.UserType.ToString(),
                roles);
        }
    }
}
