using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Mapster;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Features.Shared.Identity.Internal.Common;
using ReSys.Core.Features.Shared.Identity.Common;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Internal.Register;

public static class Register
{
    public record Request : AccountParameters
    {
        public string? PhoneNumber { get; init; }
        public DateTimeOffset? DateOfBirth { get; init; }
        public string Password { get; init; } = null!;
        public string ConfirmPassword { get; init; } = null!;
    }

    public record Response : UserProfileResponse;

    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AccountValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
        }
    }

    public class Handler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var req = command.Request;

            if (await userManager.FindByEmailAsync(req.Email) != null)
                return UserErrors.EmailAlreadyExists(req.Email);

            if (!await roleManager.RoleExistsAsync("Storefront.Customer"))
                return RoleErrors.NotFound;

            var userResult = User.Create(req.Email, req.UserName, req.FirstName, req.LastName, req.PhoneNumber);
            if (userResult.IsError) return userResult.Errors;
            var user = userResult.Value;

            var createResult = await userManager.CreateAsync(user, req.Password);
            if (!createResult.Succeeded) return createResult.Errors.ToApplicationResult(prefix: "User");

            await userManager.AddToRoleAsync(user, "Storefront.Customer");

            return user.Adapt<Response>();
        }
    }
}
