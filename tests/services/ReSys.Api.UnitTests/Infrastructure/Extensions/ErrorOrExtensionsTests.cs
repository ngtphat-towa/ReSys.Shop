using ErrorOr;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

using ReSys.Api.Infrastructure.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.UnitTests.Infrastructure.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Api")]
public class ErrorOrExtensionsTests
{
    [Fact(DisplayName = "ToApiResponse should return 200 OK with data when result is success")]
    public void ToApiResponse_GivenSuccessResult_ReturnsOkWithData()
    {
        // Arrange
        ErrorOr<string> result = "Success Data";

        // Act
        var response = result.ToApiResponse();

        // Assert
        var okResult = response.Should().BeOfType<Ok<ApiResponse<string>>>().Subject;
        okResult.Value!.Status.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Data.Should().Be("Success Data");
    }

    [Fact(DisplayName = "ToApiCreatedResponse should return 201 Created with Location header and data when result is success")]
    public void ToApiCreatedResponse_GivenSuccessResult_ReturnsCreatedWithLocation()
    {
        // Arrange
        ErrorOr<string> result = "New Resource";

        // Act
        var response = result.ToApiCreatedResponse(val => $"/api/{val}");

        // Assert
        var createdResult = response.Should().BeOfType<Created<ApiResponse<string>>>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Location.Should().Be("/api/New Resource");
        createdResult.Value!.Data.Should().Be("New Resource");
    }

    [Fact(DisplayName = "ToApiResponse should return 404 Not Found with error details when result is NotFound error")]
    public void ToApiResponse_GivenNotFoundError_ReturnsNotFound()
    {
        // Arrange
        ErrorOr<string> result = Error.NotFound("item.not_found", "Item missing");

        // Act
        var response = result.ToApiResponse();

        // Assert
        var jsonResult = response.Should().BeOfType<JsonHttpResult<ApiResponse<object>>>().Subject;
        jsonResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var body = jsonResult.Value;
        body!.Status.Should().Be(StatusCodes.Status404NotFound);
        body.Title.Should().Be("Not Found");
        body.ErrorCode.Should().Be("item.not_found");
        body.Detail.Should().Be("Item missing");
    }

    [Fact(DisplayName = "ToApiResponse should return 409 Conflict with error code when result is Conflict error")]
    public void ToApiResponse_GivenConflictError_ReturnsConflict()
    {
        // Arrange
        ErrorOr<string> result = Error.Conflict("item.conflict", "Already exists");

        // Act
        var response = result.ToApiResponse();

        // Assert
        var jsonResult = response.Should().BeOfType<JsonHttpResult<ApiResponse<object>>>().Subject;
        jsonResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        jsonResult.Value!.ErrorCode.Should().Be("item.conflict");
    }

    [Fact(DisplayName = "ToApiResponse should return 400 Bad Request with validation errors when result has validation errors")]
    public void ToApiResponse_GivenValidationError_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.Validation("Name", "Name required"),
            Error.Validation("Age", "Age invalid")
        };
        ErrorOr<string> result = errors;

        // Act
        var response = result.ToApiResponse();

        // Assert
        var badRequestResult = response.Should().BeOfType<BadRequest<ApiResponse<object>>>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var body = badRequestResult.Value;
        body!.Status.Should().Be(StatusCodes.Status400BadRequest);
        body.Title.Should().Be("Validation Failed");
        body.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain("Name required");
        body.Errors.Should().ContainKey("Age").WhoseValue.Should().Contain("Age invalid");
    }
}
