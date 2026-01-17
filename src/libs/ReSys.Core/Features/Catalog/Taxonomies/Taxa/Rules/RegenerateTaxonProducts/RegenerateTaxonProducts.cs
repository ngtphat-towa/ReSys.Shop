using ErrorOr;
using MediatR;
using ReSys.Core.Features.Catalog.Taxonomies.Services;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.RegenerateTaxonProducts;

public static class RegenerateTaxonProducts
{
    public record Command(Guid TaxonId) : IRequest<ErrorOr<Success>>;

    public class Handler(ITaxonRegenerationService regenerationService) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            return await regenerationService.RegenerateProductsForTaxonAsync(command.TaxonId, ct);
        }
    }
}