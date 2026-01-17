using Mapster;


using MediatR;


using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.GetTaxonPagedList;

public static class GetTaxonPagedList
{
    public record Request : TaxonQueryOptions;
    public record Response : TaxonListItem;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(ITaxonHierarchyService hierarchyService) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            var pagedList = await hierarchyService.GetFlatTaxonsAsync(query.Request, ct);
            
            // Convert PagedList<TaxonListItem> to PagedList<Response>
            // We use Adapt here for simple DTO-to-DTO conversion.
            var items = pagedList.Items.Select(x => x.Adapt<Response>()).ToList();

            return new PagedList<Response>(
                items,
                pagedList.TotalCount,
                pagedList.Page,
                pagedList.PageSize
            );
        }
    }
}