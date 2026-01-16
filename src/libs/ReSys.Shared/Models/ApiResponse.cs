using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using FluentValidation.Results;
using ErrorOr;

namespace ReSys.Shared.Models;

/// <summary>
/// Unified API Response wrapper inheriting from RFC 7807 ProblemDetails.
/// </summary>
public class ApiResponse<T> : ProblemDetails
{
    public bool IsSuccess { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMeta? Meta { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }

    public string? Message { get; set; }

    public ApiResponse()
    {
        Status = StatusCodes.Status200OK;
        Title = "Success";
        IsSuccess = true;
        Type = "https://tools.ietf.org/html/rfc7231#section-6.3.1";
    }

    public ApiResponse(T data, string? message = null) : this()
    {
        Data = data;
        Message = message;
    }
}

public class ApiResponse : ApiResponse<object>
{
    public ApiResponse() : base() { }
    public ApiResponse(object data, string? message = null) : base(data, message) { }

    public static ApiResponse<T> Success<T>(T data, string? message = null) => new(data, message);

    public static ApiResponse<T> Created<T>(T data, string? message = "Resource created")
    {
        return new ApiResponse<T>(data, message)
        {
            Status = StatusCodes.Status201Created,
            Title = "Resource created",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.3.2"
        };
    }

    public static ApiResponse<List<T>> Paginated<T>(PagedList<T> pagedList, string? message = null)
    {
        return new ApiResponse<List<T>>(pagedList.Items, message)
        {
            Meta = new PaginationMeta(pagedList.Page, pagedList.PageSize, pagedList.TotalCount)
        };
    }

    public static ApiResponse<object> Failure(string title, int status = StatusCodes.Status400BadRequest, string? detail = null, string? errorCode = null)
    {
        return new ApiResponse<object>
        {
            IsSuccess = false,
            Status = status,
            Title = title,
            Detail = detail,
            ErrorCode = errorCode,
            Message = detail,
            Type = $"https://httpstatuses.com/{status}"
        };
    }

    public static ApiResponse<object> ValidationFailure(IDictionary<string, string[]> errors)
    {
        return new ApiResponse<object>
        {
            IsSuccess = false,
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = "One or more validation errors occurred.",
            Errors = errors
        };
    }
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public PaginationMeta() { }

    public PaginationMeta(int page, int pageSize, int totalCount)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasNextPage = Page < TotalPages;
        HasPreviousPage = Page > 1;
    }
}

public static class ApiResponseExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this ErrorOr<T> result, string? message = null)
    {
        return result.Match(
            onValue: value => new ApiResponse<T>(value, message),
            onError: errors => CreateFailureResponse<T>(errors)
        );
    }

    public static IResult ToTypedApiResponse<T>(this ErrorOr<T> result, string? message = null)
    {
        var response = result.ToApiResponse(message);
        return TypedResults.Ok(response);
    }

    private static ApiResponse<T> CreateFailureResponse<T>(IReadOnlyList<Error> errors)
    {
        var firstError = errors[0];
        var statusCode = GetStatusCode(firstError.Type);

        var response = new ApiResponse<T>
        {
            IsSuccess = false,
            Status = statusCode,
            Title = firstError.Code,
            Detail = firstError.Description,
            ErrorCode = firstError.Code,
            Message = firstError.Description,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        if (firstError.Type == ErrorType.Validation)
        {
            response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            response.Errors = errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );
        }

        return response;
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
}
