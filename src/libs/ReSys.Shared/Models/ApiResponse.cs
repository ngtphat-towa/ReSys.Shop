using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using FluentValidation.Results;

namespace ReSys.Shared.Models;

/// <summary>
/// Unified API Response wrapper inheriting from RFC 7807 ProblemDetails.
/// Supports success data, pagination, and error details.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public class ApiResponse<T> : ProblemDetails
{
    /// <summary>
    /// The actual data payload for success responses.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    /// <summary>
    /// Pagination metadata, populated when Data is a paginated collection.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMeta? Meta { get; set; }

    /// <summary>
    /// Dictionary of validation errors, keyed by property name.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Specific error code for machine processing (e.g. "Example.NotFound").
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Only show if there's an error code
    public string? ErrorCode { get; set; }

    public ApiResponse()
    {
        // Default to 200 OK for standard instantiation
        Status = StatusCodes.Status200OK;
        Title = "Success";
        Type = "https://tools.ietf.org/html/rfc7231#section-6.3.1";
    }

    public ApiResponse(T data, string? message = null) : this()
    {
        Data = data;
        if (!string.IsNullOrEmpty(message))
        {
            Title = message;
        }
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

/// <summary>
/// Static factory for creating ApiResponse instances.
/// </summary>
public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return new ApiResponse<T>(data, message);
    }

    public static ApiResponse<T> Created<T>(T data, string? message = "Resource created")
    {
        return new ApiResponse<T>(data, message)
        {
            Status = StatusCodes.Status201Created,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.3.2"
        };
    }

    public static ApiResponse<List<T>> Paginated<T>(PagedList<T> pagedList)
    {
        return new ApiResponse<List<T>>(pagedList.Items)
        {
            Meta = new PaginationMeta(pagedList.Page, pagedList.PageSize, pagedList.TotalCount)
        };
    }

    public static ApiResponse<object> Failure(string title, int status = StatusCodes.Status400BadRequest, string? detail = null, string? errorCode = null)
    {
        return new ApiResponse<object>
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{status}",
            ErrorCode = errorCode
        };
    }

    public static ApiResponse<object> ValidationFailure(IEnumerable<ValidationFailure> failures)
    {
        var response = new ApiResponse<object>
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = "One or more validation errors occurred."
        };

        response.Errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return response;
    }

    public static ApiResponse<object> ValidationFailure(IDictionary<string, string[]> errors)
    {
        return new ApiResponse<object>
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Failed",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = "One or more validation errors occurred.",
            Errors = errors
        };
    }
}
