using Carter;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Catalog.PropertyTypes.CreatePropertyType;
using ReSys.Core.Features.Catalog.PropertyTypes.DeletePropertyType;
using ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypeDetail;
using ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypeSelectList;
using ReSys.Core.Features.Catalog.PropertyTypes.GetPropertyTypesPagedList;
using ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Catalog.PropertyTypes;

public class PropertyTypesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/catalog/property-types")
            .WithTags("Property Types");

        group.MapGet("/", async ([AsParameters] GetPropertyTypesPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetPropertyTypesPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetPropertyTypes");

        group.MapGet("/select-list", async ([AsParameters] GetPropertyTypeSelectList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetPropertyTypeSelectList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetPropertyTypeSelectList");

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetPropertyTypeDetail.Query(new GetPropertyTypeDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetPropertyTypeById");

        group.MapPost("/", async ([FromBody] CreatePropertyType.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreatePropertyType.Command(request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/catalog/property-types/{x.Id}");
        })
        .WithName("CreatePropertyType");

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdatePropertyType.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdatePropertyType.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdatePropertyType");

        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeletePropertyType.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeletePropertyType");
    }
}
