using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Catalog.Products.Classifications.Common;

namespace ReSys.Core.Features.Catalog.Products.Classifications.ManageProductClassifications;

public static class ManageProductClassifications
{
    public record Request
    {
        public List<ProductClassificationParameters> Classifications { get; set; } = [];
    }

    public record Command(Guid ProductId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleForEach(x => x.Request.Classifications).SetValidator(new ProductClassificationValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Set<Product>()
                .Include(p => p.Classifications)
                .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

            if (product == null)
                return ProductErrors.NotFound(command.ProductId);

            var requestedTaxonIds = command.Request.Classifications.Select(x => x.TaxonId).ToHashSet();
            var existingClassifications = product.Classifications.ToList();
            var changesMade = false;

            // 1. Process Removals
            foreach (var classification in existingClassifications)
            {
                if (!requestedTaxonIds.Contains(classification.TaxonId))
                {
                    var removeResult = product.RemoveClassification(classification.TaxonId);
                    if (removeResult.IsError) return removeResult.Errors;
                    
                    context.Set<Classification>().Remove(classification);
                    changesMade = true;
                }
            }

            // 2. Process Adds/Updates
            foreach (var item in command.Request.Classifications)
            {
                var existing = product.Classifications.FirstOrDefault(c => c.TaxonId == item.TaxonId);
                if (existing != null)
                {
                    if (existing.Position != item.Position)
                    {
                        var updateResult = product.UpdateClassificationPosition(item.TaxonId, item.Position);
                        if (updateResult.IsError) return updateResult.Errors;
                        
                        context.Set<Classification>().Update(existing);
                        changesMade = true;
                    }
                }
                else
                {
                    // Add new
                    var addResult = product.AddClassification(item.TaxonId, item.Position);
                    if (addResult.IsError) return addResult.Errors;

                    var newClassification = product.Classifications.First(c => c.TaxonId == item.TaxonId);
                    context.Set<Classification>().Add(newClassification);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                context.Set<Product>().Update(product);
                await context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success;
        }
    }
}
