using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Roles;

namespace ReSys.Core.UnitTests.Helpers;

public static class IdentityTestFactory
{
    public static UserManager<User> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<User>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<User>();
        var userValidators = new List<IUserValidator<User>> { new UserValidator<User>() };
        var passwordValidators = new List<IPasswordValidator<User>> { new PasswordValidator<User>() };
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var services = Substitute.For<IServiceProvider>();
        var logger = Substitute.For<ILogger<UserManager<User>>>();

        return Substitute.For<UserManager<User>>(
            store, options, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger);
    }

    public static RoleManager<Role> CreateRoleManager()
    {
        var store = Substitute.For<IRoleStore<Role>>();
        var roleValidators = new List<IRoleValidator<Role>> { new RoleValidator<Role>() };
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var logger = Substitute.For<ILogger<RoleManager<Role>>>();

        return Substitute.For<RoleManager<Role>>(
            store, roleValidators, keyNormalizer, errors, logger);
    }

    public static SignInManager<User> CreateSignInManager(UserManager<User> userManager)
    {
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<User>>();
        var options = Options.Create(new IdentityOptions());
        var logger = Substitute.For<ILogger<SignInManager<User>>>();
        var schemes = Substitute.For<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
        var confirmation = Substitute.For<IUserConfirmation<User>>();

        return Substitute.For<SignInManager<User>>(
            userManager, contextAccessor, claimsFactory, options, logger, schemes, confirmation);
    }
}