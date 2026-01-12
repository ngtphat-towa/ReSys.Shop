using ErrorOr;


using MediatR;


using Microsoft.AspNetCore.Identity;


using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Management.Users;

public static class Get
{
    public record UserDetailDto(string Id, string Email, string UserName, IList<string> Roles, IList<string> DirectPermissions);
    public record Query(string UserId) : IRequest<ErrorOr<UserDetailDto>>;

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Query, ErrorOr<UserDetailDto>>
    {
        public async Task<ErrorOr<UserDetailDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return Error.NotFound("Users.NotFound", "User not found");

            var roles = await userManager.GetRolesAsync(user);
            var claims = await userManager.GetClaimsAsync(user);
            var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

            return new UserDetailDto(user.Id, user.Email!, user.UserName!, roles, permissions);
        }
    }
}
