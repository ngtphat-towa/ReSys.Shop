using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Interfaces;
using ReSys.Infrastructure.Data;
using Pgvector.EntityFrameworkCore;
using MediatR;

namespace ReSys.Api.Features.Products;

public class ProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", async (AppDbContext context) =>
        {
            var products = await context.Products.ToListAsync();
            return Results.Ok(products);
        })
        .WithName("GetProducts");

        group.MapPost("/", async (
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] decimal price,
            IFormFile? image,
            ISender sender,
            CancellationToken ct) =>
        {
            Stream? stream = image?.OpenReadStream();
            
            var command = new ReSys.Core.Features.Products.Commands.CreateProduct.CreateProductCommand(
                name, description, price, stream, image?.FileName);

            var result = await sender.Send(command, ct);

            return result.Match(
                product => Results.Created($"/api/products/{product.Id}", product),
                errors => Results.BadRequest(errors));
        })
        .WithName("CreateProduct")
        .DisableAntiforgery();


        group.MapGet("/{id}/similar", async (Guid id, AppDbContext context) =>
        {
            var productEmbedding = await context.ProductEmbeddings
                .FirstOrDefaultAsync(pe => pe.ProductId == id);

            if (productEmbedding == null)
            {
                return Results.NotFound();
            }

            var similarProducts = await context.ProductEmbeddings
                .OrderBy(pe => pe.Embedding.L2Distance(productEmbedding.Embedding))
                .Where(pe => pe.ProductId != id)
                .Take(5)
                .Select(pe => pe.Product)
                .ToListAsync();

            return Results.Ok(similarProducts);
        })
        .WithName("GetSimilarProducts");
    }
}
