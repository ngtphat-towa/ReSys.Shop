using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Models;

namespace ReSys.Core.Common.Extensions.Query;

/// <summary>
/// Provides extension methods for paginating IQueryable collections.
/// Supports both explicit parameters and dictionary-based parameter bags.
/// </summary>
public static class PaginationExtensions
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    /// <summary>
    /// Executes a paginated query asynchronously.
    /// Returns a PagedList containing the data subset and total count.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = await query.ToPagedListAsync(
    ///     x => new ProductDto { Id = x.Id, Name = x.Name }, 
    ///     page: 1, 
    ///     pageSize: 20
    /// );
    /// </code>
    /// </example>
    /// <param name="query">The base query.</param>
    /// <param name="projection">Selection expression to map source to destination.</param>
    /// <param name="page">1-based page index.</param>
    /// <param name="pageSize">Number of items per page.</param>
    public static async Task<PagedList<TDestination>> ToPagedListAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        // 1. Sanitize inputs with defaults
        var actualPage = page is null or <= 0 ? DefaultPage : page.Value;
        var actualPageSize = pageSize is null or <= 0 ? DefaultPageSize : pageSize.Value;

        // 2. Fetch total count for metadata
        var count = await query.CountAsync(cancellationToken);

        // 3. Apply Skip/Take and execute with projection
        var items = await query
            .AsNoTracking()
            .Skip((actualPage - 1) * actualPageSize)
            .Take(actualPageSize)
            .Select(projection)
            .ToListAsync(cancellationToken);

        return new PagedList<TDestination>(items, count, actualPage, actualPageSize);
    }

    /// <summary>
    /// Returns all items if no pagination parameters are provided, otherwise returns a paged list.
    /// Useful for small collections where pagination is optional.
    /// </summary>
    public static async Task<PagedList<TDestination>> ToPagedOrAllAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (page is null && pageSize is null)
        {
            var allItems = await query
                .AsNoTracking()
                .Select(projection)
                .ToListAsync(cancellationToken);

            return new PagedList<TDestination>(allItems, allItems.Count, page: 1, pageSize: allItems.Count);
        }

        return await query.ToPagedListAsync(projection, page, pageSize, cancellationToken);
    }

    /// <summary>
    /// Paginate using a dictionary of string parameters (e.g., from a web request).
    /// Keys recognized: page, pageIndex, pageSize, limit, etc.
    /// </summary>
    public static Task<PagedList<TDestination>> ToPagedListAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        IDictionary<string, string?> parameters,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        var (page, pageSize) = ParsePaginationParams(parameters);
        return query.ToPagedListAsync(projection, page, pageSize, cancellationToken);
    }

    /// <summary>
    /// Returns all or paged items using a dictionary of string parameters.
    /// </summary>
    public static Task<PagedList<TDestination>> ToPagedOrAllAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        IDictionary<string, string?> parameters,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        var (page, pageSize) = ParsePaginationParams(parameters);
        return query.ToPagedOrAllAsync(projection, page, pageSize, cancellationToken);
    }

    /// <summary>
    /// Internal helper to extract page and pageSize from flexible naming conventions.
    /// </summary>
    private static (int? Page, int? PageSize) ParsePaginationParams(IDictionary<string, string?> parameters)
    {
        if (parameters == null) return (null, null);

        string[] pageKeys = { "page", "Page", "pageIndex", "page_index" };
        string[] sizeKeys = { "pageSize", "PageSize", "limit", "page_size" };

        int? page = null;
        int? pageSize = null;

        foreach (var key in pageKeys)
        {
            if (parameters.TryGetValue(key, out var val) && int.TryParse(val, out int p))
            {
                page = p;
                break;
            }
        }

        foreach (var key in sizeKeys)
        {
            if (parameters.TryGetValue(key, out var val) && int.TryParse(val, out int s))
            {
                pageSize = s;
                break;
            }
        }

        return (page, pageSize);
    }
}