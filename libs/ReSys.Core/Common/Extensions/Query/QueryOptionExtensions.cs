using System.Linq.Expressions;
using ReSys.Core.Common.Models;

namespace ReSys.Core.Common.Extensions;

public static class QueryOptionExtensions
{
    // --- Granular Extensions using Interfaces ---

    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, IFilterOptions? options)
    {
        if (options == null) return query;
        return query.ApplyDynamicFilter(options.Filter);
    }

    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, ISortOptions? options)
    {
        if (options == null) return query;
        return query.ApplyDynamicOrdering(options.Sort);
    }

    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> query, ISearchOptions? options)
    {
        if (options == null) return query;
        return query.ApplySearch(options.Search, options.SearchField);
    }

    public static async Task<PagedList<TDestination>> ApplyPagingAsync<TSource, TDestination>(
        this IQueryable<TSource> query, 
        IPageOptions? options,
        Expression<Func<TSource, TDestination>> projection,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (options == null) return await query.ToPagedListAsync(projection, null, null, cancellationToken);
        return await query.ToPagedListAsync(projection, options.Page, options.PageSize, cancellationToken);
    }

    // --- Composition ---

    /// <summary>
    /// Applies Filtering, Search, and Sorting to the query without executing pagination.
    /// Returns the modified IQueryable for further chaining.
    /// </summary>
    public static IQueryable<T> ApplyQueryOptions<T>(this IQueryable<T> query, QueryOptions? options)
    {
        if (options == null) return query;
        return query
            .ApplyFilter(options)
            .ApplySearch(options)
            .ApplySort(options);
    }

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