using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Models;
using System.Linq.Expressions;

namespace ReSys.Core.Common.Extensions;

public static class QueryableExtensions
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    public static async Task<PagedList<TDestination>> ToPagedListAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        // Fallback to defaults if page or pageSize is null or invalid
        var actualPage = page is null or <= 0 ? DefaultPage : page.Value;
        var actualPageSize = pageSize is null or <= 0 ? DefaultPageSize : pageSize.Value;

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip((actualPage - 1) * actualPageSize)
            .Take(actualPageSize)
            .Select(projection)
            .ToListAsync(cancellationToken);

        return new PagedList<TDestination>(items, count, actualPage, actualPageSize);
    }

    /// <summary>
    /// Returns all items if both page and pageSize are null.
    /// Otherwise returns a paged result with default fallbacks (page=1, pageSize=10).
    /// </summary>
    public static async Task<PagedList<TDestination>> ToPagedOrAllAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        // If BOTH are null, return all items (no pagination)
        if (page is null && pageSize is null)
        {
            var allItems = await query
                .AsNoTracking()
                .Select(projection)
                .ToListAsync(cancellationToken);

            return new PagedList<TDestination>(allItems, allItems.Count, page: 1, pageSize: allItems.Count);
        }

        // Otherwise, use paged result with defaults
        return await query.ToPagedListAsync(projection, page, pageSize, cancellationToken);
    }
}
