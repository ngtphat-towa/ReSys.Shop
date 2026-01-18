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
    public record Request : TaxonInput;
    public record Response : TaxonDetail;
    public record Command(Guid TaxonomyId, Request Request) : IRequest<ErrorOr<Response>>;

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

            var result = taxonomy.AddTaxon(request.Name, request.ParentId);

            if (result.IsError)
                return result.Errors;

            var taxon = result.Value;

            var updateResult = taxon.Update(
                request.Name, 
                request.Presentation, 
                request.Description, 
                request.Slug, 
                request.Automatic);

            if (updateResult.IsError) return updateResult.Errors;

            taxon.PublicMetadata = request.PublicMetadata;
            taxon.PrivateMetadata = request.PrivateMetadata;
            taxon.MetaTitle = request.MetaTitle;
            taxon.MetaDescription = request.MetaDescription;
            taxon.MetaKeywords = request.MetaKeywords;

            context.Set<Taxon>().Add(taxon);
            context.Set<Taxonomy>().Update(taxonomy);

            await context.SaveChangesAsync(cancellationToken);

            // 1. Hierarchy: Rebuild via events

            // 2. Products: Regenerate via events

            return await context.Set<Taxon>()
                .AsNoTracking()
                .Where(x => x.Id == taxon.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}