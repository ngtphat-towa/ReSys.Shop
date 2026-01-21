using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Features.Shared.Identity.Account.Addresses;
using ReSys.Core.Features.Shared.Identity.Account.Common;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace ReSys.Core.UnitTests.Features.Identity.Account.Addresses;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Addresses")]
public class CreateAddressTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    static CreateAddressTests()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<UserAddress, AccountAddressResponse>();
    }

    [Fact(DisplayName = "Handle: Should successfully add address to user book")]
    public async Task Handle_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("cust@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user);

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(user.Id);

        var handler = new CreateAddress.Handler(_userContext, context, userManager);
        var request = new CreateAddress.Request {
            Address1 = "123 Street", City = "Prague", ZipCode = "11000", CountryCode = "CZ", 
            FirstName = "John", LastName = "Doe", IsDefault = false, Type = AddressType.Shipping, Label = "Home"
        };

        // Act
        var result = await handler.Handle(new CreateAddress.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? $"Error: {result.FirstError.Code} - {result.FirstError.Description}" : "");
        
        var dbUser = await context.Users.Include(u => u.UserAddresses).FirstAsync(u => u.Id == user.Id, TestContext.Current.CancellationToken);
        dbUser.UserAddresses.Should().HaveCount(1);
    }
}
