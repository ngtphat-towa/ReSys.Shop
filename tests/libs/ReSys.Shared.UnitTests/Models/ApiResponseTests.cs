using Microsoft.AspNetCore.Http;

using ErrorOr;

using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Shared.UnitTests.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class ApiResponseTests
{
    [Fact(DisplayName = "ApiResponse.Success: Should create response with data and 200 OK")]
    public void Success_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        var data = new { Name = "Test" };
        var message = "Success Message";

        // Act
        var response = ApiResponse.Success(data, message);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be(message);
        response.Status.Should().Be(StatusCodes.Status200OK);
    }

    [Fact(DisplayName = "ApiResponse.Paginated: Should create response with pagination metadata and data")]
    public void Paginated_ShouldReturnSuccessfulPaginatedResponse()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        var pagedList = new PagedList<string>(items, 10, 1, 2);

        // Act
        var response = ApiResponse.Paginated(pagedList, "Paging Success");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(items);
        response.Meta.Should().NotBeNull();
        response.Meta!.Page.Should().Be(1);
        response.Meta.PageSize.Should().Be(2);
        response.Meta.TotalCount.Should().Be(10);
        response.Meta.TotalPages.Should().Be(5);
        response.Meta.HasNextPage.Should().BeTrue();
        response.Meta.HasPreviousPage.Should().BeFalse();
    }

    [Fact(DisplayName = "ToApiResponse: From ErrorOr value should return success")]
    public void ToApiResponse_FromErrorOrValue_ShouldReturnSuccess()
    {
        // Arrange
        ErrorOr<string> result = "Success Data";

        // Act
        var response = result.ToApiResponse("Custom Message");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Be("Success Data");
        response.Message.Should().Be("Custom Message");
    }

    [Fact(DisplayName = "ToApiResponse: From ErrorOr error should return failure")]
    public void ToApiResponse_FromErrorOrError_ShouldReturnFailure()
    {
        // Arrange
        var error = Error.NotFound("Test.NotFound", "The resource was not found");
        ErrorOr<string> result = error;

        // Act
        var response = result.ToApiResponse();

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Status.Should().Be(StatusCodes.Status404NotFound);
        response.ErrorCode.Should().Be("Test.NotFound");
        response.Message.Should().Be("The resource was not found");
    }

    [Fact(DisplayName = "ToApiResponse: From validation errors should populate errors dictionary")]
    public void ToApiResponse_FromValidationErrors_ShouldPopulateErrorsDictionary()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.Validation("Field1", "Error 1"),
            Error.Validation("Field1", "Error 2"),
            Error.Validation("Field2", "Error 3")
        };
        ErrorOr<string> result = errors;

        // Act
        var response = result.ToApiResponse();

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Status.Should().Be(StatusCodes.Status400BadRequest);
        response.Errors.Should().NotBeNull();
        response.Errors.Should().HaveCount(2);
        response.Errors!["Field1"].Should().Contain("Error 1");
        response.Errors["Field1"].Should().Contain("Error 2");
        response.Errors["Field2"].Should().Contain("Error 3");
    }
}
