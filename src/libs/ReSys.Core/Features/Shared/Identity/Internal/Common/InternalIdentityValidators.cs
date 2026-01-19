using FluentValidation;

using ReSys.Core.Domain.Identity.Users;

namespace ReSys.Core.Features.Shared.Identity.Internal.Common;

public abstract class AccountValidator<T> : AbstractValidator<T> where T : AccountParameters
{
    protected AccountValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(UserErrors.EmailRequired.Code)
            .MaximumLength(UserConstraints.EmailMaxLength);

        RuleFor(x => x.UserName)
            .MaximumLength(UserConstraints.UserNameMaxLength)
            .Matches(UserConstraints.UserNamePattern)
            .When(x => !string.IsNullOrEmpty(x.UserName));

        RuleFor(x => x.FirstName)
            .MaximumLength(UserConstraints.FirstNameMaxLength);

        RuleFor(x => x.LastName)
            .MaximumLength(UserConstraints.LastNameMaxLength);
    }
}
