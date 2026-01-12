using System.Security.Claims;
using FluentAssertions;
using OpenIddict.Abstractions;
using ReSys.Identity.Authentication;
using Xunit;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ReSys.Identity.UnitTests.Authentication;

public class OpenIddictHelpersTests
{
    [Theory]
    [InlineData(Claims.Name, true)]
    [InlineData(Claims.Role, true)]
    [InlineData(Claims.Email, true)]
    [InlineData("custom_claim", false)]
    public void GetDestinations_ShouldIncludeIdentityToken_ForStandardClaims(string claimType, bool expectedInIdToken)
    {
        // Arrange
        var claim = new Claim(claimType, "test-value");

        // Act
        var destinations = OpenIddictHelpers.GetDestinations(claim).ToList();

        // Assert
        destinations.Should().Contain(Destinations.AccessToken);
        if (expectedInIdToken)
        {
            destinations.Should().Contain(Destinations.IdentityToken);
        }
        else
        {
            destinations.Should().NotContain(Destinations.IdentityToken);
        }
    }

    [Fact]
    public void GetDestinations_ShouldIncludeAccessToken_ForRoleClaim()
    {
        // Arrange
        var claim = new Claim(Claims.Role, "Admin");

        // Act
        var destinations = OpenIddictHelpers.GetDestinations(claim).ToList();

        // Assert
        destinations.Should().Contain(Destinations.AccessToken);
    }
}
