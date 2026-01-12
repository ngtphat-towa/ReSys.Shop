using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Common.Constants;
using ReSys.Identity.Presentation.Extensions;
using ReSys.Infrastructure.Authentication.Authorization;

namespace ReSys.Identity.Features.Management.Users;

public class UsersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/management/users")
            .WithTags("Management - Users");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new List.Query(), ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok(result.Value);
        })
        .RequirePermission(AppPermissions.Identity.Users.List);

        group.MapGet("/{userId}", async (string userId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Get.Query(userId), ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok(result.Value);
        })
        .RequirePermission(AppPermissions.Identity.Users.View);

        group.MapPut("/{userId}/roles", async (string userId, [FromBody] UpdateRoles.Request request, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdateRoles.Command(userId, request);
            var result = await sender.Send(command, ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .RequirePermission(AppPermissions.Identity.Users.ManageRoles);

        group.MapPut("/{userId}/permissions", async (string userId, [FromBody] UpdatePermissions.Request request, ISender sender, CancellationToken ct) =>
        {
            var command = new UpdatePermissions.Command(userId, request);
            var result = await sender.Send(command, ct);
            return result.IsError ? result.ToApiResponse() : Results.Ok();
        })
        .WithName("UpdateUserDirectPermissions")
        .RequirePermission(AppPermissions.Identity.Users.ManagePermissions);
    }
}
