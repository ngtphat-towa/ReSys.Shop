using MediatR;

using Microsoft.AspNetCore.Identity;

using ReSys.Core.Domain.Identity.Users;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.Users.GetUserRoles;

public static class GetUserRoles
{
    public record Response
    {
        public List<string> Roles { get; init; } = [];
    }

    public record Query(string UserId) : IRequest<ErrorOr<Response>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query request, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return UserErrors.NotFound(request.UserId);

            var roles = await userManager.GetRolesAsync(user);

            return new Response { Roles = roles.ToList() };
        }
    }
}