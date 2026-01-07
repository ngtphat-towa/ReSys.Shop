using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Common.Models;

namespace ReSys.Api.UnitTests.Infrastructure.Extensions;

public class ErrorOrExtensionsTests
{
    [Fact(DisplayName = "ToApiResponse (Success): Should return 200 OK with data")]
    public void ToApiResponse_OnSuccess_ShouldReturnOkWithData()
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

    [Fact(DisplayName = "ToApiCreatedResponse (Success): Should return 201 Created with Location header and data")]
    public void ToApiCreatedResponse_OnSuccess_ShouldReturnCreatedWithLocation()
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

    [Fact(DisplayName = "ToApiResponse (NotFound): Should return 404 Not Found with error details")]
    public void ToApiResponse_OnNotFoundError_ShouldReturnNotFound()
    {
        // Arrange
        ErrorOr<string> result = Error.NotFound("item.not_found", "Item missing");

        // Act
        var response = result.ToApiResponse();

        // Assert
        // Results.Json returns JsonHttpResult<ApiResponse<object>>
        var jsonResult = response.Should().BeOfType<JsonHttpResult<ApiResponse<object>>>().Subject;
        jsonResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        
        var body = jsonResult.Value;
        body!.Status.Should().Be(StatusCodes.Status404NotFound);
        body.Title.Should().Be("Not Found");
        body.ErrorCode.Should().Be("item.not_found");
        body.Detail.Should().Be("Item missing");
    }

    [Fact(DisplayName = "ToApiResponse (Conflict): Should return 409 Conflict with error code")]
    public void ToApiResponse_OnConflictError_ShouldReturnConflict()
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

    [Fact(DisplayName = "ToApiResponse (Validation): Should return 400 Bad Request with validation errors dictionary")]
    public void ToApiResponse_OnValidationError_ShouldReturnBadRequestWithErrors()
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
        // Validation errors use Results.BadRequest(...) which is BadRequest<ApiResponse<object>>
        var badRequestResult = response.Should().BeOfType<BadRequest<ApiResponse<object>>>().Subject;
        badRequestResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var body = badRequestResult.Value;
        body!.Status.Should().Be(StatusCodes.Status400BadRequest);
        body.Title.Should().Be("Validation Failed");
        body.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain("Name required");
        body.Errors.Should().ContainKey("Age").WhoseValue.Should().Contain("Age invalid");
    }
}
