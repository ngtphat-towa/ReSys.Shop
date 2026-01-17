using Carter;

using Microsoft.AspNetCore.Mvc;

using MediatR;

using ReSys.Core.Features.Testing.Examples.CreateExample;
using ReSys.Core.Features.Testing.Examples.GetExamples;
using ReSys.Core.Features.Testing.Examples.GetSimilarExamples;
using ReSys.Core.Features.Testing.Examples.GetExampleById;
using ReSys.Core.Features.Testing.Examples.UpdateExample;
using ReSys.Core.Features.Testing.Examples.DeleteExample;
using ReSys.Core.Features.Testing.Examples.UpdateExampleImage;
using ReSys.Shared.Constants;
using ReSys.Api.Infrastructure.Extensions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Examples;

/// <summary>
/// Module for managing example entities and their associated images.
/// </summary>
public class ExamplesModule : ICarterModule
{
    /// <summary>
    /// Registers endpoints for example management.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // API: Group all example-related endpoints under a common prefix
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/testing/examples")
            .WithTags("Examples");

        // NOTE: [AsParameters] binds automatically after our normalization middleware runs.
        // API: Retrieve a paginated list of examples
        group.MapGet("/", async ([AsParameters] GetExamples.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetExamples.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetExamples");

        // API: Retrieve a paginated list of examples using advanced filtering (V2)
        group.MapGet("/v2", async ([AsParameters] GetExamplesV2.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetExamplesV2.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetExamplesV2");

        // API: Retrieve a single example by its unique identifier
        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetExampleById.Query(new GetExampleById.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetExampleById");

        // API: Create a new example entity
        group.MapPost("/", async (
            [FromBody] CreateExample.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateExample.Command(request), ct);
            return result.ToApiCreatedResponse(example => $"{RouteConstants.ApiPrefix}/testing/examples/{example.Id}");
        })
        .WithName("CreateExample");

        // API: Update an existing example entity
        group.MapPut("/{id}", async (
            Guid id,
            [FromBody] UpdateExample.Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateExample.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateExample");

        // BUS: Handle image upload and processing for a specific example
        // API: Upload and associate an image with an example
        group.MapPost("/{id}/image", async (
            Guid id,
            IFormFile image,
            ISender sender,
            CancellationToken ct) =>
        {
            using var stream = image.OpenReadStream();

            var request = new UpdateExampleImage.Request(id, stream, image.FileName);
            var result = await sender.Send(new UpdateExampleImage.Command(request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateExampleImage")
        .DisableAntiforgery();

        // API: Delete an example entity by its identifier
        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteExample.Command(id));

            if (result.IsError)
            {
                return result.ToApiResponse();
            }

            return Results.NoContent();
        })
        .WithName("DeleteExample");

        // BUS: Retrieve examples that are semantically similar based on ML features
        // API: Retrieve similar examples for a given example ID
        group.MapGet("/{id}/similar", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetSimilarExamples.Query(new GetSimilarExamples.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetSimilarExamples");
    }
}
