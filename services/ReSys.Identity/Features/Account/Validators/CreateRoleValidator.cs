using FluentValidation;
using ReSys.Identity.Features.Account.Contracts;

namespace ReSys.Identity.Features.Account.Validators;

public class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
    }
}
