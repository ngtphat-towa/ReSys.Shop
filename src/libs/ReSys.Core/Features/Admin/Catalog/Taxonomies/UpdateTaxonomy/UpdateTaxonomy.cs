using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Common;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.UpdateTaxonomy;

public static class UpdateTaxonomy
{
    // Request:
    public record Request : TaxonomyInput;

    // Response:
    public record Response : TaxonomyDetail;

    // Command:
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    private class RequestValidator : TaxonomyValidator<Request> { }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // 1. Get: domain entity
            var taxonomy = await context.Set<Taxonomy>()
                .Include(x => x.Taxons)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (taxonomy is null)
                return TaxonomyErrors.NotFound(command.Id);

            // 2. Check: name conflict
            if (taxonomy.Name != request.Name && await context.Set<Taxonomy>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return TaxonomyErrors.DuplicateName;
            }

            // 3. Update: domain entity
            var updateResult = taxonomy.Update(
                name: request.Name,
                presentation: request.Presentation,
                position: request.Position);

            if (updateResult.IsError)
                return updateResult.Errors;

            // 4. Set: metadata
            taxonomy.PublicMetadata = request.PublicMetadata;
            taxonomy.PrivateMetadata = request.PrivateMetadata;

            // 5. Save
            context.Set<Taxonomy>().Update(taxonomy);
            await context.SaveChangesAsync(cancellationToken);

            // Return: projected response
            return taxonomy.Adapt<Response>();
        }
    }
}
