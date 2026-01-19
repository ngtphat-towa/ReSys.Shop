using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Admin.Users.Common;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Admin.Users.UpdateAdminUser;

public static class UpdateAdminUser
{
    public record Request : AccountParameters
    {
        public string? PhoneNumber { get; init; }
    }

    public record Response : AdminUserDetailResponse;

    public record Command(string Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(command.Id);
            if (user == null) return UserErrors.NotFound(command.Id);

            var req = command.Request;

            // 1. Update Core Profile via Domain Method
            user.UpdateProfile(req.FirstName, req.LastName, user.DateOfBirth, user.ProfileImagePath);
            user.PhoneNumber = req.PhoneNumber;
            
            if (user.Email != req.Email)
            {
                user.Email = req.Email;
                user.UserName = req.UserName ?? req.Email;
                user.NormalizedEmail = req.Email.ToUpperInvariant();
                user.NormalizedUserName = user.UserName.ToUpperInvariant();
            }

            // 2. Persistence
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User");

            return user.Adapt<Response>();
        }
    }
}
