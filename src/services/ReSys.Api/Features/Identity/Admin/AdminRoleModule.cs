using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Identity.Admin.Roles.GetRolesPagedList;
using ReSys.Core.Features.Identity.Admin.Roles.CreateRole;
using ReSys.Core.Features.Identity.Admin.Roles.UpdateRole;
using ReSys.Core.Features.Identity.Admin.Roles.DeleteRole;
using ReSys.Core.Features.Identity.Admin.Roles.AssignPermissionToRole;
using ReSys.Core.Features.Identity.Admin.Roles.UnassignPermissionFromRole;
using ReSys.Core.Features.Identity.Admin.Roles.SyncRolePermissions;
using ReSys.Core.Features.Identity.Admin.Roles.GetUsersInRole;
using ReSys.Infrastructure.Security.Authorization;
using ReSys.Shared.Constants.Permissions;
using ReSys.Shared.Models.Wrappers;
using Mapster;

namespace ReSys.Api.Features.Identity.Admin;

public sealed class AdminRoleModule : ICarterModule
{
    private const string BaseRoute = "api/admin/identity/roles";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Identity - Admin Roles")
            .RequireAuthorization();

        group.MapGet("/", GetRolesHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.List);
        group.MapPost("/", CreateRoleHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.Create);
        group.MapPut("{id}", UpdateRoleHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.Update);
        group.MapDelete("{id}", DeleteRoleHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.Delete);
        
        // Users in Role
        group.MapGet("{roleName}/users", GetUsersInRoleHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.View);

        // Permissions
        group.MapPost("{id}/permissions", AssignPermissionHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.AssignPermission);
        group.MapPut("{id}/permissions", SyncPermissionsHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.AssignPermission);
        group.MapDelete("{id}/permissions/{permissionName}", UnassignPermissionHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.Role.UnassignPermission);
    }

    private static async Task<IResult> GetRolesHandler([AsParameters] GetRolesPagedList.Request options, ISender mediator)
    {
        var result = await mediator.Send(new GetRolesPagedList.Query(options));
        return result.ToTypedApiResponse("Roles retrieved");
    }

    private static async Task<IResult> CreateRoleHandler([FromBody] CreateRole.Request request, ISender mediator)
    {
        var result = await mediator.Send(new CreateRole.Command(request));
        return result.ToTypedApiResponse("Role created");
    }

    private static async Task<IResult> UpdateRoleHandler([FromRoute] string id, [FromBody] UpdateRole.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateRole.Command(id, request));
        return result.ToTypedApiResponse("Role updated");
    }

    private static async Task<IResult> DeleteRoleHandler([FromRoute] string id, ISender mediator)
    {
        var result = await mediator.Send(new DeleteRole.Command(id));
        return result.ToTypedApiResponse("Role deleted");
    }

    private static async Task<IResult> GetUsersInRoleHandler([FromRoute] string roleName, [AsParameters] GetUsersInRole.Request options, ISender mediator)
    {
        var result = await mediator.Send(new GetUsersInRole.Query(roleName, options));
        return result.ToTypedApiResponse("Users in role retrieved");
    }

    private static async Task<IResult> AssignPermissionHandler([FromRoute] string id, [FromBody] AssignPermissionToRole.Request request, ISender mediator)
    {
        var result = await mediator.Send(new AssignPermissionToRole.Command(id, request));
        return result.ToTypedApiResponse("Permission assigned to role");
    }

    private static async Task<IResult> SyncPermissionsHandler([FromRoute] string id, [FromBody] SyncRolePermissions.Request request, ISender mediator)
    {
        var result = await mediator.Send(new SyncRolePermissions.Command(id, request));
        return result.ToTypedApiResponse("Role permissions synchronized");
    }

    private static async Task<IResult> UnassignPermissionHandler([FromRoute] string id, [FromRoute] string permissionName, ISender mediator)
    {
        var result = await mediator.Send(new UnassignPermissionFromRole.Command(id, permissionName));
        return result.ToTypedApiResponse("Permission unassigned from role");
    }
}