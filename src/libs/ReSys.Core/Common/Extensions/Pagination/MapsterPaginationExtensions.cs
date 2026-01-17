using Microsoft.EntityFrameworkCore;

using Mapster;


using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Common.Extensions.Pagination;

/// <summary>
/// Provides extension methods for paginating IQueryable collections using Mapster for automatic projection.
/// </summary>
public static class MapsterPaginationExtensions
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    /// <summary>
    /// Executes a paginated query asynchronously using Mapster for projection.
    /// </summary>
    public static async Task<PagedList<TDestination>> ToPagedListAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        IPageOptions? options,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        var (page, pageSize) = ResolvePagination(options?.Page, options?.PageSize);
        var count = await query.CountAsync(cancellationToken);

        var items = await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectToType<TDestination>()
            .ToListAsync(cancellationToken);

        return new PagedList<TDestination>(items, count, page, pageSize);
    }

    /// <summary>
    /// Returns all items projected via Mapster if no pagination parameters are provided, otherwise returns a paged list.
    /// </summary>
    public static async Task<PagedList<TDestination>> ToPagedOrAllAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        IPageOptions? options,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (options?.Page is null && options?.PageSize is null)
        {
            var items = await query.AsNoTracking().ProjectToType<TDestination>().ToListAsync(cancellationToken);
            return new PagedList<TDestination>(items, items.Count, 1, items.Count);
        }

        return await query.ToPagedListAsync<TSource, TDestination>(options, cancellationToken);
    }

    /// <summary>
    /// Returns an empty list if no pagination parameters are provided, otherwise returns a paged list (Mapster).
    /// </summary>
    public static async Task<PagedList<TDestination>> ToPagedOrEmptyAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        IPageOptions? options,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        if (options?.Page is null && options?.PageSize is null)
        {
            return new PagedList<TDestination>([], 0, 1, 0);
        }

        return await query.ToPagedListAsync<TSource, TDestination>(options, cancellationToken);
    }

    private static (int Page, int PageSize) ResolvePagination(int? page, int? pageSize)
    {
        var actualPage = page is null or <= 0 ? DefaultPage : page.Value;
        var actualPageSize = pageSize is null or <= 0 ? DefaultPageSize : pageSize.Value;
        return (actualPage, actualPageSize);
    }
}
