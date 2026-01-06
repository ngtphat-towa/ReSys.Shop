using FluentValidation;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Api.Features.Account.Validators;

public class CreateRoleValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
    }
}
