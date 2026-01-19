using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Admin.Users.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Admin.Users.UpdateStaffProfile;

public static class UpdateStaffProfile
{
    public record Request(string? EmployeeId, string? Department, string? JobTitle);
    public record Response : AdminUserDetailResponse;
    public record Command(string UserId, Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var user = await userManager.Users
                .Include(u => u.StaffProfile)
                .FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

            if (user == null) return UserErrors.NotFound(command.UserId);

            // 1. Ensure profile exists or create it
            user.EnsureStaffProfile(command.Request.EmployeeId);
            
            // 2. Domain Logic: Update employment details
            var updateResult = user.StaffProfile!.Update(
                command.Request.Department, 
                command.Request.JobTitle, 
                command.Request.EmployeeId);

            if (updateResult.IsError) return updateResult.Errors;

            // 3. Persistence
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "User.Staff");

            return user.Adapt<Response>();
        }
    }
}
