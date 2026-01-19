using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Common;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.UpdateTaxon;

public static class UpdateTaxon
{
    public record Request : TaxonInput;
    public record Response : TaxonDetail;
    public record Command(Guid TaxonomyId, Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    private class RequestValidator : TaxonInputValidator { }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TaxonomyId).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var taxonomy = await context.Set<Taxonomy>()
                .Include(t => t.Taxons)
                .FirstOrDefaultAsync(t => t.Id == command.TaxonomyId, cancellationToken);

            if (taxonomy == null)
                return TaxonomyErrors.NotFound(command.TaxonomyId);

            var taxon = taxonomy.Taxons.FirstOrDefault(t => t.Id == command.Id);
            if (taxon == null)
                return TaxonErrors.NotFound(command.Id);

            // Update: taxon
            var updateResult = taxon.Update(
                request.Name,
                request.Presentation,
                request.Description,
                request.Slug,
                request.Automatic);

            if (updateResult.IsError)
                return updateResult.Errors;

            // Set Parent if changed
            if (taxon.ParentId != request.ParentId)
            {
                var parentResult = taxon.SetParent(request.ParentId);
                if (parentResult.IsError) return parentResult.Errors;
            }

            // Set: metadata
            taxon.PublicMetadata = request.PublicMetadata;
            taxon.PrivateMetadata = request.PrivateMetadata;
            
            // Set: SEO
            taxon.MetaTitle = request.MetaTitle;
            taxon.MetaDescription = request.MetaDescription;
            taxon.MetaKeywords = request.MetaKeywords;

            context.Set<Taxon>().Update(taxon);
            context.Set<Taxonomy>().Update(taxonomy);

            await context.SaveChangesAsync(cancellationToken);

            return taxon.Adapt<Response>();
        }
    }
}
