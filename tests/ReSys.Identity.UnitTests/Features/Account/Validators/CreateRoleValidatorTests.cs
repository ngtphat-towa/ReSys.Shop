using FluentAssertions;
using ReSys.Identity.Features.Account.Contracts;
using ReSys.Identity.Features.Account.Validators;
using Xunit;

namespace ReSys.Identity.UnitTests.Features.Account.Validators;

public class CreateRoleValidatorTests
{
    private readonly CreateRoleValidator _sut = new();

    [Fact]
    public async Task Validate_ShouldReturnTrue_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateRoleRequest("Admin");

        // Act
        var result = await _sut.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldReturnFalse_WhenNameIsEmpty()
    {
        // Arrange
        var request = new CreateRoleRequest("");

        // Act
        var result = await _sut.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateRoleRequest.Name));
    }
}
