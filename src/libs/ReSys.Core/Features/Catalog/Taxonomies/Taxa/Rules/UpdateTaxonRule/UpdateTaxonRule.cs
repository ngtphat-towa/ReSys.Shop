using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.UpdateTaxonRule;

public static class UpdateTaxonRule
{
    public record Request
    {
        public string Type { get; init; } = null!;
        public string Value { get; init; } = null!;
        public string? MatchPolicy { get; init; }
        public string? PropertyName { get; init; }
    }

    public record Response : TaxonRuleResponse;

    public record Command(Guid TaxonomyId, Guid TaxonId, Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Type).NotEmpty();
            RuleFor(x => x.Request.Value).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var rule = await context.Set<TaxonRule>()
                .Include(x => x.Taxon)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.TaxonId == command.TaxonId, cancellationToken);

            if (rule is null)
                return TaxonRuleErrors.NotFound(command.Id);

            var updateResult = rule.Update(
                request.Type,
                request.Value,
                request.MatchPolicy,
                request.PropertyName);

            if (updateResult.IsError)
                return updateResult.Errors;

            rule.Taxon.MarkedForRegenerateProducts = true;

            await context.SaveChangesAsync(cancellationToken);

            return rule.Adapt<Response>();
        }
    }
}
