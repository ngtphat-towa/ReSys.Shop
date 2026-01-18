using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Catalog.Taxonomies.Common;
using ReSys.Core.Features.Catalog.Taxonomies.Services;

namespace ReSys.Core.Features.Catalog.Taxonomies.CreateTaxonomy;

public static class CreateTaxonomy
{
    // Request:
    public record Request : TaxonomyInput;

    // Response:
    public record Response : TaxonomyDetail;

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new TaxonomyInputValidator());
        }
    }

    // Handler:
    public class Handler(
        IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: duplicate name
            if (await context.Set<Taxonomy>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return TaxonomyErrors.DuplicateName;
            }

            // Create: domain entity
            var taxonomyResult = Taxonomy.Create(
                name: request.Name,
                presentation: request.Presentation,
                position: request.Position);

            if (taxonomyResult.IsError)
                return taxonomyResult.Errors;

            var taxonomy = taxonomyResult.Value;

            // Set: metadata
            taxonomy.PublicMetadata = request.PublicMetadata;
            taxonomy.PrivateMetadata = request.PrivateMetadata;

            // Save: domain entity
            context.Set<Taxonomy>().Add(taxonomy);
            
            if (taxonomy.RootTaxon != null)
            {
                context.Set<ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Taxon>().Add(taxonomy.RootTaxon);
            }

            await context.SaveChangesAsync(cancellationToken);

            // Hierarchy: initialization via events

            // Return: projected response
            return taxonomy.Adapt<Response>();
        }
    }
}