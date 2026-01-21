using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.Roles;
using ReSys.Core.Domain.Identity.Users.Logins;
using ReSys.Core.Domain.Identity.Users.Claims;
using ReSys.Core.Domain.Identity.Users.Tokens;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Domain.Identity.Roles.Claims;
using ReSys.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.UnitTests.TestInfrastructure;

namespace ReSys.Core.UnitTests.Helpers;

public static class IdentityTestFactory
{
    public static (AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<Role> roleManager) CreateRealIdentityContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new TestAppDbContext(options);

        var userStore = new UserStore<User, Role, AppDbContext, string, UserClaim, UserRole, UserLogin, UserToken, RoleClaim>(context);
        var roleStore = new RoleStore<Role, AppDbContext, string, UserRole, RoleClaim>(context);
        
        var identityOptions = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<User>();
        var userValidators = new List<IUserValidator<User>> { new UserValidator<User>() };
        var passwordValidators = new List<IPasswordValidator<User>> { new PasswordValidator<User>() };
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        
        var userManager = new UserManager<User>(
            userStore, identityOptions, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, Substitute.For<IServiceProvider>(), Substitute.For<ILogger<UserManager<User>>>());

        var roleManager = new RoleManager<Role>(
            roleStore, new List<IRoleValidator<Role>> { new RoleValidator<Role>() }, keyNormalizer, errors, Substitute.For<ILogger<RoleManager<Role>>>());

        var signInManager = new SignInManager<User>(
            userManager, Substitute.For<IHttpContextAccessor>(), Substitute.For<IUserClaimsPrincipalFactory<User>>(), identityOptions, Substitute.For<ILogger<SignInManager<User>>>(), Substitute.For<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(), Substitute.For<IUserConfirmation<User>>());

        return (context, userManager, signInManager, roleManager);
    }

    // Standard Mock Managers for simple logic tests
    public static UserManager<User> CreateUserManager() => CreateRealIdentityContext().userManager;
    public static SignInManager<User> CreateSignInManager(UserManager<User> userManager) => CreateRealIdentityContext().signInManager;
    public static RoleManager<Role> CreateRoleManager() => CreateRealIdentityContext().roleManager;
}
