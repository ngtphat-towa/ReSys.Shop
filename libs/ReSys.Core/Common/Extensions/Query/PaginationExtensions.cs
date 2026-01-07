using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Models;

namespace ReSys.Core.Common.Extensions;

public static class PaginationExtensions
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
