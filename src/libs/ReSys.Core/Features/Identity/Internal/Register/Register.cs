using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

using FluentValidation;

using Mapster;

using ReSys.Core.Common.Notifications.Interfaces;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Features.Identity.Internal.Common;
using ReSys.Core.Features.Identity.Common;

using ErrorOr;

namespace ReSys.Core.Features.Identity.Internal.Register;

public static class Register
{
    // Request: Full profile registration
    public record Request : AccountParameters
    {
        public string? PhoneNumber { get; init; }
        public DateTimeOffset? DateOfBirth { get; init; }
        public string Password { get; init; } = null!;
        public string ConfirmPassword { get; init; } = null!;
    }

    // Response:
    public record Response : UserProfileResponse;

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    // Validator: Matches temp logic
    public class Validator : AccountValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
                .WithMessage("Passwords do not match.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(UserConstraints.PhoneMaxLength)
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTimeOffset.UtcNow)
                .WithMessage("Date of birth must be in the past.")
                .When(x => x.DateOfBirth.HasValue);
        }
    }

    // Handler:
    public class Handler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        INotificationService notificationService,
        IConfiguration configuration) : IRequestHandler<Command, ErrorOr<Response>>
    {
        private const string DefaultCustomerRole = "Storefront.Customer";

        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // 1. Triple Uniqueness Check (Addresses Gap #1)
            if (await userManager.FindByEmailAsync(request.Email) != null)
                return UserErrors.EmailAlreadyExists(request.Email);

            if (!string.IsNullOrEmpty(request.UserName) && await userManager.FindByNameAsync(request.UserName) != null)
                return UserErrors.UserNameAlreadyExists(request.UserName);

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var phoneExists = await userManager.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
                if (phoneExists) return UserErrors.PhoneNumberAlreadyExists(request.PhoneNumber);
            }

            // 2. Role Integrity Guard (Addresses Gap #2)
            if (!await roleManager.RoleExistsAsync(DefaultCustomerRole))
                return RoleErrors.NotFound;

            // 3. Aggregate creation
            var userResult = User.Create(
                request.Email,
                request.UserName,
                request.FirstName,
                request.LastName,
                request.PhoneNumber);

            if (userResult.IsError) return userResult.Errors;
            var user = userResult.Value;
            user.DateOfBirth = request.DateOfBirth; // Addresses Gap #3

            // 4. Save to Identity Store
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return result.Errors.ToApplicationResult(prefix: "Register");

            try
            {
                // 5. Role assignment
                var roleResult = await userManager.AddToRoleAsync(user, DefaultCustomerRole);
                if (!roleResult.Succeeded) throw new Exception("Role assignment failed");

                // 6. Trigger Dual Verification Flow
                // Send Email
                var emailResult = await userManager.GenerateAndSendConfirmationEmailAsync(
                    notificationService,
                    configuration,
                    user,
                    cancellationToken: cancellationToken);

                if (emailResult.IsError) throw new Exception("Email notification failed");

                // Send SMS if phone exists
                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var smsResult = await userManager.GenerateAndSendConfirmationSmsAsync(
                        notificationService,
                        configuration,
                        user,
                        cancellationToken: cancellationToken);

                    if (smsResult.IsError) throw new Exception("SMS notification failed");
                }
            }
            catch (Exception)
            {
                // Addresses Gap #4: Granular Atomic Rollback
                await userManager.DeleteAsync(user);

                return Error.Failure(
                    code: "Register.AtomicFailure",
                    description: "User creation rolled back due to role assignment or notification failure.");
            }

            return user.Adapt<Response>();
        }
    }
}