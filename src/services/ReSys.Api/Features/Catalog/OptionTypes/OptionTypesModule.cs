using Carter;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using ReSys.Api.Infrastructure.Extensions;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.CreateOptionType;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.DeleteOptionType;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.GetOptionTypeDetail;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.GetOptionTypeSelectList;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.GetOptionTypesPagedList;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.UpdateOptionType;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.CreateOptionValue;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.DeleteOptionValue;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.GetOptionValuesPagedList;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.UpdateOptionValue;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.UpdateOptionValuePositions;
using ReSys.Shared.Constants;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Catalog.OptionTypes;

public class OptionTypesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"{RouteConstants.ApiPrefix}/catalog/option-types")
            .WithTags("Option Types");

        #region Option Types
        group.MapGet("/", async ([AsParameters] GetOptionTypesPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetOptionTypesPagedList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetOptionTypes");

        group.MapGet("/select-list", async ([AsParameters] GetOptionTypeSelectList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetOptionTypeSelectList.Query(request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetOptionTypeSelectList");

        group.MapGet("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetOptionTypeDetail.Query(new GetOptionTypeDetail.Request(id)));
            return result.ToApiResponse();
        })
        .WithName("GetOptionTypeById");

        group.MapPost("/", async ([FromBody] CreateOptionType.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateOptionType.Command(request), ct);
            return result.ToApiCreatedResponse(x => $"{RouteConstants.ApiPrefix}/catalog/option-types/{x.Id}");
        })
        .WithName("CreateOptionType");

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateOptionType.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateOptionType.Command(id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateOptionType");

        group.MapDelete("/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteOptionType.Command(id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteOptionType");
        #endregion

        #region Option Values
        group.MapGet("/{optionTypeId}/values", async (Guid optionTypeId, [AsParameters] GetOptionValuesPagedList.Request request, ISender sender) =>
        {
            var result = await sender.Send(new GetOptionValuesPagedList.Query(optionTypeId, request));
            return Results.Ok(ApiResponse.Paginated(result));
        })
        .WithName("GetOptionValues");

        group.MapPost("/{optionTypeId}/values", async (Guid optionTypeId, [FromBody] CreateOptionValue.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateOptionValue.Command(optionTypeId, request), ct);
            return result.ToApiResponse();
        })
        .WithName("CreateOptionValue");

        group.MapPut("/{optionTypeId}/values/{id}", async (Guid optionTypeId, Guid id, [FromBody] UpdateOptionValue.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateOptionValue.Command(optionTypeId, id, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateOptionValue");

        group.MapDelete("/{optionTypeId}/values/{id}", async (Guid optionTypeId, Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteOptionValue.Command(optionTypeId, id));
            if (result.IsError) return result.ToApiResponse();
            return Results.NoContent();
        })
        .WithName("DeleteOptionValue");

        group.MapPut("/{optionTypeId}/values/positions", async (Guid optionTypeId, [FromBody] UpdateOptionValuePositions.Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateOptionValuePositions.Command(optionTypeId, request), ct);
            return result.ToApiResponse();
        })
        .WithName("UpdateOptionValuePositions");
        #endregion
    }
}
