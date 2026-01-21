using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;

namespace ReSys.Core.Features.Shared.Identity.Account.Profile;

public static class UpdateProfile
{
    public record Request(
        string? FirstName,
        string? LastName,
        DateTimeOffset? DateOfBirth,
        string? ProfileImagePath,
        bool? AcceptsMarketing,
        string? PreferredLocale,
        string? PreferredCurrency);

    public record Response : UserProfileResponse;

    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(
        IUserContext userContext,
        UserManager<User> userManager,
        IApplicationDbContext context)
        : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return UserErrors.Unauthorized;

            var user = await userManager.Users
                .Include(u => u.CustomerProfile)
                .Include(u => u.StaffProfile)
                .FirstOrDefaultAsync(u => u.Id == userContext.UserId, ct);

            if (user == null) return UserErrors.NotFound(userContext.UserId);

            var req = command.Request;

            user.UpdateProfile(req.FirstName, req.LastName, req.DateOfBirth, req.ProfileImagePath);

            if (user.CustomerProfile != null)
            {
                user.CustomerProfile.SetPreferences(
                    req.AcceptsMarketing ?? user.CustomerProfile.AcceptsMarketing,
                    req.PreferredLocale ?? user.CustomerProfile.PreferredLocale,
                    req.PreferredCurrency ?? user.CustomerProfile.PreferredCurrency
                );
                context.Set<CustomerProfile>().Update(user.CustomerProfile);
                await context.SaveChangesAsync(ct);
            }

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "Profile");

            return user.Adapt<Response>();
        }
    }
}