using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Core.Features.Identity.Users.CreateUser;

public static class CreateUser
{
    public record Command(CreateUserRequest Request) : IRequest<ErrorOr<string>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }

        private class RequestValidator : AbstractValidator<CreateUserRequest>
        {
            public RequestValidator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
                RuleFor(x => x.FirstName).NotEmpty();
                RuleFor(x => x.LastName).NotEmpty();
            }
        }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<string>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserType = request.UserType,
                EmailConfirmed = true // Auto-confirm for now
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            }

            return user.Id;
        }
    }
}
