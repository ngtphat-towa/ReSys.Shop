using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Management.Users;

public static class List
{
    public record UserDto(string Id, string Email, string UserName);
    public record Query() : IRequest<ErrorOr<List<UserDto>>>;

    public class Handler(UserManager<ApplicationUser> userManager) : IRequestHandler<Query, ErrorOr<List<UserDto>>>
    {
        public async Task<ErrorOr<List<UserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await userManager.Users
                .AsNoTracking()
                .Select(u => new UserDto(u.Id, u.Email!, u.UserName!))
                .ToListAsync(cancellationToken);
            return users;
        }
    }
}
