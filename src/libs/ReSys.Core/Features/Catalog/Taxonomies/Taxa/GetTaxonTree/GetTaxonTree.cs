using ErrorOr;
using MediatR;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.GetTaxonTree;

public static class GetTaxonTree
{
    public record Request : TaxonQueryOptions;
    public record Response : TaxonTreeResponse;
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(ITaxonHierarchyService hierarchyService) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken ct)
        {
            var tree = await hierarchyService.BuildTaxonTreeAsync(query.Request, ct);
            
            return new Response
            {
                Tree = tree.Tree,
                Breadcrumbs = tree.Breadcrumbs,
                FocusedNode = tree.FocusedNode,
                FocusedSubtree = tree.FocusedSubtree
            };
        }
    }
}