using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Mapster;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.CreateTaxon;

public static class CreateTaxon
{
    // Request:
    public record Request : TaxonInput;

    // Response:
    public record Response : TaxonDetail;

    // Command:
    public record Command(Guid TaxonomyId, Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    private class RequestValidator : TaxonInputValidator { }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    // Handler:
    public class Handler(IApplicationDbContext context, Features.Catalog.Taxonomies.Services.ITaxonHierarchyService hierarchyService) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // 1. Get Taxonomy (with taxons to check for root)
            var taxonomy = await context.Set<Taxonomy>()
                .Include(x => x.Taxons)
                .FirstOrDefaultAsync(x => x.Id == command.TaxonomyId, cancellationToken);

            if (taxonomy is null)
                return TaxonomyErrors.NotFound(command.TaxonomyId);

            // 2. Add via Root Logic in Domain
            var addResult = taxonomy.AddTaxon(request.Name, request.ParentId);

            if (addResult.IsError)
                return addResult.Errors;

            var taxon = addResult.Value;

            // 3. Set: additional fields
            taxon.Presentation = request.Presentation;
            taxon.Description = request.Description;
            taxon.HideFromNav = request.HideFromNav;
            taxon.Automatic = request.Automatic;
            taxon.RulesMatchPolicy = request.RulesMatchPolicy;
            taxon.SortOrder = request.SortOrder;
            taxon.ImageUrl = request.ImageUrl;
            taxon.SquareImageUrl = request.SquareImageUrl;
            taxon.MetaTitle = request.MetaTitle;
            taxon.MetaDescription = request.MetaDescription;
            taxon.MetaKeywords = request.MetaKeywords;
            taxon.PublicMetadata = request.PublicMetadata;
            taxon.PrivateMetadata = request.PrivateMetadata;

            // Ensure permalink is updated (requires parent permalink or taxonomy name)
            if (taxon.ParentId != null)
            {
                var parent = taxonomy.Taxons.First(t => t.Id == taxon.ParentId);
                taxon.Parent = parent;
            }
            taxon.UpdatePermalink(taxonomy.Name);

            // 4. Save
            context.Set<Taxon>().Add(taxon);
            await context.SaveChangesAsync(cancellationToken);

            // 5. Rebuild hierarchy
            await hierarchyService.RebuildAsync(command.TaxonomyId, cancellationToken);

            // 6. Return: projected response
            return taxon.MapToDetail<Response>();
        }
    }
}