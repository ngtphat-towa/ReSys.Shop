using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Models;
using System.Linq.Expressions;

namespace ReSys.Core.Common.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedList<TDestination>> ToPagedListAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        Expression<Func<TSource, TDestination>> projection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(projection)
            .ToListAsync(cancellationToken);

        return new PagedList<TDestination>(items, count, page, pageSize);
    }
}
