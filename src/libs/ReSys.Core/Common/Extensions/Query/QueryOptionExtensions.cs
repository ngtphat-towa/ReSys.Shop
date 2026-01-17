using System.Linq.Expressions;

using ReSys.Core.Common.Extensions.Filters;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Common.Extensions.Search;
using ReSys.Core.Common.Extensions.Sort;
using ReSys.Shared.Models;
using ReSys.Shared.Models.Filters;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Search;

namespace ReSys.Core.Common.Extensions.Query;

/// <summary>
/// Main entry point for applying QueryOptions and their granular components to IQueryable.
/// Provides a chainable, type-safe API for building complex queries from DTOs.
/// </summary>
public static class QueryOptionExtensions
{
    // --- Granular Extensions using Interfaces ---

    /// <summary>
    /// Applies dynamic filtering based on the provided IFilterOptions.
    /// </summary>
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, IFilterOptions? options)
    {
        if (options == null) return query;
        return query.ApplyDynamicFilter(options.Filter);
    }

    /// <summary>
    /// Applies dynamic sorting based on the provided ISortOptions.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, ISortOptions? options)
    {
        if (options == null) return query;
        return query.ApplyDynamicOrdering(options.Sort);
    }

    /// <summary>
    /// Applies global search based on the provided ISearchOptions.
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, ISearchOptions? options)
    {
        if (options == null) return query;
        return query.ApplySearch(options.Search, options.SearchField);
    }

    /// <summary>
    /// Finalizes the query by applying pagination and executing asynchronously.
    /// Returns a PagedList result.
    /// </summary>
    public static async Task<PagedList<TDestination>> ApplyPagingAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        IPageOptions? options,
        Expression<Func<TSource, TDestination>> projection,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        // If options are null, return first page with defaults
        if (options == null) return await query.ToPagedListAsync(projection, null, null, cancellationToken);
        return await query.ToPagedListAsync(projection, options.Page, options.PageSize, cancellationToken);
    }

    // --- Composition ---

    /// <summary>
    /// Applies Filtering, Search, and Sorting to the query without executing pagination.
    /// Returns the modified IQueryable for further chaining (e.g., adding custom Where clauses).
    /// </summary>
    /// <example>
    /// <code>
    /// var result = await _context.Products
    ///     .ApplyQueryOptions(options) // Apply Filter, Sort, Search
    ///     .Where(x => !x.IsDeleted)   // Add custom logic
    ///     .ApplyPagingAsync(options, x => x.ToDto()); // Finalize with Paging
    /// </code>
    /// </example>
    public static IQueryable<T> ApplyQueryOptions<T>(this IQueryable<T> query, QueryOptions? options)
    {
        if (options == null) return query;
        return query
            .ApplyFilter(options)
            .ApplySearch(options)
            .ApplySort(options);
    }

    /// <summary>
    /// Applies all QueryOptions (Filter, Search, Sort, Page) and executes asynchronously.
    /// Uses identity projection (returns source items).
    /// </summary>
    public static async Task<PagedList<T>> ApplyQueryOptionsAsync<T>(
        this IQueryable<T> query,
        QueryOptions? options,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (options == null) return await query.ToPagedListAsync(x => x, null, null, cancellationToken);
        var result = query.ApplyQueryOptions(options);
        return await result.ToPagedListAsync(x => x, options.Page, options.PageSize, cancellationToken);
    }

    /// <summary>
    /// Applies all QueryOptions (Filter, Search, Sort, Page) and executes asynchronously with projection.
    /// </summary>
    public static async Task<PagedList<TDestination>> ApplyQueryOptionsAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        QueryOptions? options,
        Expression<Func<TSource, TDestination>> projection,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (options == null) return await query.ToPagedListAsync(projection, null, null, cancellationToken);
        var result = query.ApplyQueryOptions(options);
        return await result.ToPagedListAsync(projection, options.Page, options.PageSize, cancellationToken);
    }
}
