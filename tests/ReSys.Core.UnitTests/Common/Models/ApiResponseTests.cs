using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ReSys.Core.Common.Models;
using Xunit;

namespace ReSys.Core.UnitTests.Common.Models;

public class ApiResponseTests
{
    [Fact(DisplayName = "ApiResponse.Success: Should create response with data and 200 OK")]
    public void Success_ShouldCreateResponseWithDataAnd200OK()
    {
        // Arrange
        var data = new { Name = "Test" };

        // Act
        var response = ApiResponse.Success(data);

        // Assert
        response.Status.Should().Be(StatusCodes.Status200OK);
        response.Data.Should().Be(data);
        response.Title.Should().Be("Success");
        response.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.3.1");
    }

    [Fact(DisplayName = "ApiResponse.Created: Should create response with data and 201 Created")]
    public void Created_ShouldCreateResponseWithDataAnd201Created()
    {
        // Arrange
        var data = new { Id = 1 };

        // Act
        var response = ApiResponse.Created(data);

        // Assert
        response.Status.Should().Be(StatusCodes.Status201Created);
        response.Data.Should().Be(data);
        response.Title.Should().Be("Resource created");
        response.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.3.2");
    }

    [Fact(DisplayName = "ApiResponse.Paginated: Should create response with pagination metadata and data")]
    public void Paginated_ShouldCreateResponseWithMetaAndData()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2" };
        var pagedList = new PagedList<string>(items, 10, 1, 2);

        // Act
        var response = ApiResponse.Paginated(pagedList);

        // Assert
        response.Status.Should().Be(StatusCodes.Status200OK);
        response.Data.Should().BeEquivalentTo(items);
        response.Meta.Should().NotBeNull();
        response.Meta!.Page.Should().Be(1);
        response.Meta.PageSize.Should().Be(2);
        response.Meta.TotalCount.Should().Be(10);
        response.Meta.TotalPages.Should().Be(5); // 10 / 2 = 5
        response.Meta.HasNextPage.Should().BeTrue();
    }

    [Fact(DisplayName = "ApiResponse.Failure: Should create response with status code, title, detail, and error code")]
    public void Failure_ShouldCreateResponseWithStatusAndErrorCode()
    {
        // Arrange
        var errorCode = "Test.Error";
        var detail = "Something went wrong";

        // Act
        var response = ApiResponse.Failure("Error Title", StatusCodes.Status400BadRequest, detail, errorCode);

        // Assert
        response.Status.Should().Be(StatusCodes.Status400BadRequest);
        response.ErrorCode.Should().Be(errorCode);
        response.Detail.Should().Be(detail);
        response.Title.Should().Be("Error Title");
        response.Type.Should().Be("https://httpstatuses.com/400");
    }

    [Fact(DisplayName = "ApiResponse.ValidationFailure: Should create response with 400 Bad Request and validation errors")]
    public void ValidationFailure_ShouldCreateResponseWithErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error1" } }
        };

        // Act
        var response = ApiResponse.ValidationFailure(errors);

        // Assert
        response.Status.Should().Be(StatusCodes.Status400BadRequest);
        response.Errors.Should().BeEquivalentTo(errors);
        response.Title.Should().Be("Validation Failed");
        response.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
    }
}
