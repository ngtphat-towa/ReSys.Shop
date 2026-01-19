using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Rules.AddTaxonRule;

public static class AddTaxonRule
{
    public record Request : TaxonRuleInput;
    public record Response : TaxonRuleResponse;
    public record Command(Guid TaxonomyId, Guid TaxonId, Request Request) : IRequest<ErrorOr<Response>>;

    private class RequestValidator : TaxonRuleInputValidator { }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.TaxonId).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var taxon = await context.Set<Domain.Catalog.Taxonomies.Taxa.Taxon>()
                .Include(x => x.TaxonRules)
                .FirstOrDefaultAsync(x => x.Id == command.TaxonId && x.TaxonomyId == command.TaxonomyId, cancellationToken);

            if (taxon is null)
                return ReSys.Core.Domain.Catalog.Taxonomies.Taxa.TaxonErrors.NotFound(command.TaxonId);

            var result = taxon.AddRule(
                request.Type,
                request.Value,
                request.MatchPolicy,
                request.PropertyName);

            if (result.IsError)
                return result.Errors;

            var rule = result.Value;

            context.Set<TaxonRule>().Add(rule);
            context.Set<Domain.Catalog.Taxonomies.Taxa.Taxon>().Update(taxon);

            await context.SaveChangesAsync(cancellationToken);

            return rule.Adapt<Response>();
        }
    }
}
