using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Account.Common;
using ErrorOr;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Account.Profile;

public static class GetProfile
{
    public record Query : IRequest<ErrorOr<FullProfileResponse>>;

    public class Handler(
        IUserContext userContext,
        UserManager<User> userManager) : IRequestHandler<Query, ErrorOr<FullProfileResponse>>
    {
        public async Task<ErrorOr<FullProfileResponse>> Handle(Query request, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return UserErrors.Unauthorized;

            var user = await userManager.Users
                .Include(u => u.CustomerProfile)
                .Include(u => u.StaffProfile)
                .FirstOrDefaultAsync(u => u.Id == userContext.UserId, ct);

            if (user == null) return UserErrors.NotFound(userContext.UserId);

            return user.Adapt<FullProfileResponse>();
        }
    }
}
