using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Identity.Domain;

namespace ReSys.Identity.Features.Account;

public static class Register
{
    public record Request(string Email, string Password, string ConfirmPassword);

    public record Command(Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Data.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.Data.ConfirmPassword)
                .Equal(x => x.Data.Password)
                .WithMessage("The password and confirmation password do not match.");
        }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<Success>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser { UserName = request.Data.Email, Email = request.Data.Email };
            var result = await _userManager.CreateAsync(user, request.Data.Password);

            if (result.Succeeded) return Result.Success;
            
            var errors = result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
            return errors;
        }
    }
}