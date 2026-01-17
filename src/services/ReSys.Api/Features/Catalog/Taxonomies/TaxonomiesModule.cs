using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Catalog.Taxonomies.CreateTaxonomy;
using ReSys.Core.Features.Catalog.Taxonomies.DeleteTaxonomy;
using ReSys.Core.Features.Catalog.Taxonomies.GetTaxonomiesPagedList;
using ReSys.Core.Features.Catalog.Taxonomies.GetTaxonomyDetail;
using ReSys.Core.Features.Catalog.Taxonomies.GetTaxonomySelectList;
using ReSys.Core.Features.Catalog.Taxonomies.UpdateTaxonomy;
using ReSys.Core.Features.Catalog.Taxonomies.RebuildTaxonomy;
using ReSys.Core.Features.Catalog.Taxonomies.ValidateTaxonomy;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.CreateTaxon;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.DeleteTaxon;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.GetTaxonDetail;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.GetTaxonPagedList;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.GetTaxonTree;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.UpdateTaxon;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.UpdateTaxonPositions;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.AddTaxonRule;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.DeleteTaxonRule;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.GetTaxonRules;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.UpdateTaxonRule;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.UpdateTaxonRules;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.RegenerateTaxonProducts;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Catalog.Taxonomies;

public class TaxonomiesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/catalog/taxonomies")
            .WithTags("Taxonomies");

        #region Taxonomies
        group.MapGet("/", async ([AsParameters] GetTaxonomiesPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetTaxonomiesPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetTaxonomies");

        group.MapGet("/select-list", async ([AsParameters] GetTaxonomySelectList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetTaxonomySelectList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetTaxonomySelectList");

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetTaxonomyDetail.Query(new GetTaxonomyDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetTaxonomyById");

        group.MapPost("/", async ([FromBody] CreateTaxonomy.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateTaxonomy.Command(request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/catalog/taxonomies/{x.Id}");
        })
        .WithName("CreateTaxonomy");

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateTaxonomy.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTaxonomy.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateTaxonomy");

        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteTaxonomy.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteTaxonomy");

        group.MapPost("/{id}/rebuild", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RebuildTaxonomy.Command(id), ct);
            return result.ToApiResponse();
        })
        .WithName("RebuildTaxonomy");

        group.MapGet("/{id}/validate", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ValidateTaxonomy.Query(id), ct);
            return result.ToApiResponse();
        })
        .WithName("ValidateTaxonomy");
        #endregion

        #region Taxons
        group.MapGet("/{taxonomyId}/taxons", async (Guid taxonomyId, [AsParameters] GetTaxonPagedList.Request request, ISender sender) =>
        {
            request.TaxonomyId = [taxonomyId];
            var result = await sender.Send(new GetTaxonPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetTaxons");

        group.MapGet("/{taxonomyId}/taxons/tree", async (Guid taxonomyId, [AsParameters] GetTaxonTree.Request request, ISender sender) =>
        {
            request.TaxonomyId = [taxonomyId];
            var result = await sender.Send(new GetTaxonTree.Query(request));
            return result.ToApiResponse();
        })
        .WithName("GetTaxonTree");

        group.MapGet("/{taxonomyId}/taxons/{id}", async (Guid taxonomyId, Guid id, ISender sender) =>
        {
            // Note: TaxonomyId validation could be added in handler if needed, 
            // but currently GetTaxonDetail only takes Id.
            var result = await sender.Send(new GetTaxonDetail.Query(new GetTaxonDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetTaxonById");

        group.MapPost("/{taxonomyId}/taxons", async (Guid taxonomyId, [FromBody] CreateTaxon.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateTaxon.Command(taxonomyId, request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/catalog/taxonomies/{taxonomyId}/taxons/{x.Id}");
        })
        .WithName("CreateTaxon");

        group.MapPut("/{taxonomyId}/taxons/{id}", async (Guid taxonomyId, Guid id, [FromBody] UpdateTaxon.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTaxon.Command(taxonomyId, id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateTaxon");

        group.MapPut("/{taxonomyId}/taxons/positions", async (Guid taxonomyId, [FromBody] UpdateTaxonPositions.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTaxonPositions.Command(taxonomyId, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateTaxonPositions");

        group.MapDelete("/{taxonomyId}/taxons/{id}", async (Guid taxonomyId, Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteTaxon.Command(taxonomyId, id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteTaxon");
        #endregion

        #region Taxon Rules
        group.MapGet("/{taxonomyId}/taxons/{taxonId}/rules", async (Guid taxonomyId, Guid taxonId, ISender sender) =>
        {
            var result = await sender.Send(new GetTaxonRules.Query(taxonomyId, taxonId));
            return result.ToApiResponse();
        })
        .WithName("GetTaxonRules");

        group.MapPost("/{taxonomyId}/taxons/{taxonId}/rules", async (Guid taxonomyId, Guid taxonId, [FromBody] AddTaxonRule.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AddTaxonRule.Command(taxonomyId, taxonId, request), ct);
            return result.ToApiResponse();
        })
        .WithName("AddTaxonRule");

        group.MapPut("/{taxonomyId}/taxons/{taxonId}/rules", async (Guid taxonomyId, Guid taxonId, [FromBody] UpdateTaxonRules.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTaxonRules.Command(taxonomyId, taxonId, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateTaxonRules");

        group.MapPut("/{taxonomyId}/taxons/{taxonId}/rules/{id}", async (Guid taxonomyId, Guid taxonId, Guid id, [FromBody] UpdateTaxonRule.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTaxonRule.Command(taxonomyId, taxonId, id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateTaxonRule");

        group.MapDelete("/{taxonomyId}/taxons/{taxonId}/rules/{id}", async (Guid taxonomyId, Guid taxonId, Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteTaxonRule.Command(taxonomyId, taxonId, id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteTaxonRule");

        group.MapPost("/{taxonomyId}/taxons/{taxonId}/rules/regenerate", async (Guid taxonomyId, Guid taxonId, ISender sender, CancellationToken ct) =>
        {
            // Note: Command only needs TaxonId based on its definition
            var result = await sender.Send(new RegenerateTaxonProducts.Command(taxonId), ct);
            return result.ToApiResponse();
        })
        .WithName("RegenerateTaxonProducts");
        #endregion
    }
}
