using ErrorOr;

using ReSys.Core.Common.Extensions;
using ReSys.Shared.Models;

namespace ReSys.Api.Infrastructure.Extensions;

public static class ErrorOrExtensions
{
    public static IResult ToApiResponse<T>(this ErrorOr<T> result)
    {
        return result.Match(
            value => Results.Ok(ApiResponse.Success(value)),
            errors => MapErrors(errors)
        );
    }

    public static IResult ToApiCreatedResponse<T>(this ErrorOr<T> result, Func<T, string> uriGenerator)
    {
        return result.Match(
             value => Results.Created(uriGenerator(value), ApiResponse.Created(value)),
             errors => MapErrors(errors)
        );
    }

    private static IResult MapErrors(List<Error> errors)
    {
        if (errors.Count == 0)
        {
            // Should not happen if IsError is true
            return Results.Problem();
        }

        var firstError = errors[0];

        // Map ErrorOr ErrorType to HttpStatusCode
        var loops = errors.Select(e => e.Type).Distinct().ToList();

        var status = firstError.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        // Special handling for Validation errors to return dictionary
        if (firstError.Type == ErrorType.Validation)
        {
            var validationErrors = errors
                .Where(e => e.Code != "General")
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Results.BadRequest(ApiResponse.ValidationFailure(validationErrors));
        }

        // Return generic failure for other types
        var title = firstError.Type switch
        {
            ErrorType.Conflict => "Conflict",
            ErrorType.Validation => "Validation Error",
            ErrorType.NotFound => "Not Found",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            _ => "ServerError"
        };

        return Results.Json(
            ApiResponse.Failure(
                title: title,
                status: status,
                detail: firstError.Description,
                errorCode: firstError.Code.ToSnakeCase()),
            statusCode: status
        );
    }
}
