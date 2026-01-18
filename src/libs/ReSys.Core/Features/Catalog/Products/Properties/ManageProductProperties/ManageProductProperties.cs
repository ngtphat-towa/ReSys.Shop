using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Domain.Catalog.Products.Properties;
using ReSys.Core.Features.Catalog.Products.Properties.Common;

namespace ReSys.Core.Features.Catalog.Products.Properties.ManageProductProperties;

public static class ManageProductProperties
{
    public record Request
    {
        public List<ProductPropertyParameters> Properties { get; set; } = [];
    }

    public record Command(Guid ProductId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleForEach(x => x.Request.Properties).SetValidator(new ProductPropertyValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Set<Product>()
                .Include(p => p.ProductProperties)
                .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

            if (product == null)
                return ProductErrors.NotFound(command.ProductId);

            var requestedTypeIds = command.Request.Properties.Select(x => x.PropertyTypeId).ToHashSet();
            var existingProperties = product.ProductProperties.ToList();
            var changesMade = false;

            // 1. Removals
            foreach (var prop in existingProperties)
            {
                if (!requestedTypeIds.Contains(prop.PropertyTypeId))
                {
                    var removeResult = product.RemoveProperty(prop.PropertyTypeId);
                    if (removeResult.IsError) return removeResult.Errors;
                    
                    context.Set<ProductProperty>().Remove(prop);
                    changesMade = true;
                }
            }

            // 2. Adds/Updates
            if (command.Request.Properties.Any())
            {
                var propertyTypes = await context.Set<PropertyType>()
                    .Where(pt => requestedTypeIds.Contains(pt.Id))
                    .ToListAsync(cancellationToken);

                foreach (var propReq in command.Request.Properties)
                {
                    var existing = product.ProductProperties.FirstOrDefault(p => p.PropertyTypeId == propReq.PropertyTypeId);
                    var propType = propertyTypes.FirstOrDefault(pt => pt.Id == propReq.PropertyTypeId);
                    
                    if (propType == null) continue;

                    if (existing != null)
                    {
                        if (existing.Value != propReq.Value)
                        {
                            var updateResult = product.SetPropertyValue(propType, propReq.Value);
                            if (updateResult.IsError) return updateResult.Errors;
                            
                            context.Set<ProductProperty>().Update(existing);
                            changesMade = true;
                        }
                    }
                    else
                    {
                        var addResult = product.SetPropertyValue(propType, propReq.Value);
                        if (addResult.IsError) return addResult.Errors;

                        var newProp = product.ProductProperties.First(p => p.PropertyTypeId == propReq.PropertyTypeId);
                        context.Set<ProductProperty>().Add(newProp);
                        changesMade = true;
                    }
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
