using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Core.Features.Shared.Identity.Account.Addresses;
using ReSys.Core.Features.Shared.Identity.Account.Common;
using ReSys.Core.UnitTests.Helpers;
using ReSys.Core.UnitTests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.UnitTests.Features.Identity.Account.Addresses;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Addresses")]
public class AddressTests(TestDatabaseFixture fixture) : IdentityTestBase(fixture)
{
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();

    static AddressTests()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<UserAddress, AccountAddressResponse>();
        TypeAdapterConfig.GlobalSettings.NewConfig<UserAddress, AddressSelectListItem>();
    }

    [Fact(DisplayName = "UpdateAddress: Should successfully update existing address")]
    public async Task UpdateAddress_ValidRequest_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("addr@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user);

        var addressVal = Address.Create("Old Street", "Old City", "12345", "US").Value;
        var userAddress = UserAddress.Create(user.Id, addressVal, "Old Label", AddressType.Shipping).Value;
        user.AddAddress(userAddress);
        await userManager.UpdateAsync(user);

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(user.Id);

        var handler = new UpdateAddress.Handler(_userContext, userManager);
        var request = new UpdateAddress.Request {
            Address1 = "New Street", City = "New City", ZipCode = "54321", CountryCode = "CZ",
            FirstName = "New", LastName = "Name", Label = "New Label", Type = AddressType.Billing
        };

        // Act
        var result = await handler.Handle(new UpdateAddress.Command(userAddress.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Address1.Should().Be("New Street");
        result.Value.Label.Should().Be("New Label");
    }

    [Fact(DisplayName = "DeleteAddress: Should successfully remove address")]
    public async Task DeleteAddress_ValidId_ShouldSucceed()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userManager = sp.GetRequiredService<UserManager<User>>();
        
        var user = User.Create("del@example.com").Value;
        user.Id = Guid.NewGuid().ToString();
        await userManager.CreateAsync(user);

        var addressVal = Address.Create("Street", "City", "12345", "US").Value;
        var userAddress = UserAddress.Create(user.Id, addressVal, "Label").Value;
        user.AddAddress(userAddress);
        await userManager.UpdateAsync(user);

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(user.Id);

        var handler = new DeleteAddress.Handler(_userContext, userManager);

        // Act
        var result = await handler.Handle(new DeleteAddress.Command(userAddress.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsError.Should().BeFalse();
        var dbUser = await context.Users.Include(u => u.UserAddresses).FirstAsync(u => u.Id == user.Id);
        dbUser.UserAddresses.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetAddressesPagedList: Should return user addresses only")]
    public async Task GetAddressesPagedList_Should_ReturnUserAddresses()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        
        var userId = Guid.NewGuid().ToString();
        var otherUserId = Guid.NewGuid().ToString();

        var addressVal = Address.Create("Street", "City", "12345", "US").Value;
        
        var userAddr = UserAddress.Create(userId, addressVal, "User Address").Value;
        var otherAddr = UserAddress.Create(otherUserId, addressVal, "Other Address").Value;

        context.Set<UserAddress>().AddRange(userAddr, otherAddr);
        await context.SaveChangesAsync();

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(userId);

        var handler = new GetAddressesPagedList.Handler(context, _userContext);

        // Act
        var result = await handler.Handle(new GetAddressesPagedList.Query(new GetAddressesPagedList.Request()), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.First().Label.Should().Be("User Address");
    }

    [Fact(DisplayName = "GetAddressSelectList: Should return mapped items")]
    public async Task GetAddressSelectList_Should_ReturnMappedItems()
    {
        // Arrange
        var (context, sp) = CreateTestContext();
        var userId = Guid.NewGuid().ToString();
        var addressVal = Address.Create("Street", "City", "12345", "US").Value;
        var userAddr = UserAddress.Create(userId, addressVal, "Home").Value;

        context.Set<UserAddress>().Add(userAddr);
        await context.SaveChangesAsync();

        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(userId);

        var handler = new GetAddressSelectList.Handler(context, _userContext);

        // Act
        var result = await handler.Handle(new GetAddressSelectList.Query(new QueryOptions()), TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.First().Label.Should().Be("Home");
    }
}
