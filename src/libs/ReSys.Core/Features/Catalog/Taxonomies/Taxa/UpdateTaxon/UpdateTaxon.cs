using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.UpdateTaxon;

public static class UpdateTaxon
{
    // Request:
    public record Request : TaxonInput;

    // Response:
    public record Response : TaxonDetail;

    // Command:
    public record Command(Guid TaxonomyId, Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

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

            // 1. Uniqueness check
            if (await context.Set<Taxon>().AnyAsync(x => 
                x.TaxonomyId == command.TaxonomyId && 
                x.ParentId == request.ParentId &&
                x.Name == request.Name && 
                x.Id != command.Id, cancellationToken))
            {
                return TaxonErrors.DuplicateName;
            }

            // 2. Get: domain entity
            var taxon = await context.Set<Taxon>()
                .Include(x => x.Taxonomy)
                .Include(x => x.Parent)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.TaxonomyId == command.TaxonomyId, cancellationToken);

            if (taxon is null)
                return TaxonErrors.NotFound(command.Id);

            // Prevent: changing root taxon's parent or taxonomy
            if (taxon.ParentId == null && (request.ParentId != null || command.TaxonomyId != taxon.TaxonomyId))
            {
                return TaxonErrors.RootLock;
            }

            var parentChanged = taxon.ParentId != request.ParentId;
            var positionChanged = taxon.Position != request.Position;

            // 3. Update: domain entity
            var updateResult = taxon.Update(
                name: request.Name,
                presentation: request.Presentation,
                description: request.Description,
                slug: request.Slug,
                automatic: request.Automatic,
                hideFromNav: request.HideFromNav,
                rulesMatchPolicy: request.RulesMatchPolicy,
                sortOrder: request.SortOrder,
                imageUrl: request.ImageUrl,
                squareImageUrl: request.SquareImageUrl);

            if (updateResult.IsError)
                return updateResult.Errors;

            // 4. Update: parent and permalink
            if (parentChanged)
            {
                var parentResult = taxon.SetParent(request.ParentId);
                if (parentResult.IsError) return parentResult.Errors;
                
                if (request.ParentId.HasValue)
                {
                    taxon.Parent = await context.Set<Taxon>().FindAsync([request.ParentId.Value], cancellationToken);
                }
                else
                {
                    taxon.Parent = null;
                }
            }
            
            taxon.UpdatePermalink(taxon.Taxonomy.Name);

            // 5. Set: additional fields
            taxon.Position = request.Position;
            taxon.MetaTitle = request.MetaTitle;
            taxon.MetaDescription = request.MetaDescription;
            taxon.MetaKeywords = request.MetaKeywords;
            taxon.PublicMetadata = request.PublicMetadata;
            taxon.PrivateMetadata = request.PrivateMetadata;

            // 6. Save
            context.Set<Taxon>().Update(taxon);
            await context.SaveChangesAsync(cancellationToken);

            // 7. Rebuild hierarchy if needed
            if (parentChanged || positionChanged)
            {
                await hierarchyService.RebuildAsync(command.TaxonomyId, cancellationToken);
            }

            // 8. Return: projected response
            return taxon.MapToDetail<Response>();
        }
    }
}