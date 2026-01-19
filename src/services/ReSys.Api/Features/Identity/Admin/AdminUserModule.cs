using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Shared.Identity.Admin.Users.GetAdminUsersPagedList;
using ReSys.Core.Features.Shared.Identity.Admin.Users.GetAdminUserDetail;
using ReSys.Core.Features.Shared.Identity.Admin.Users.CreateAdminUser;
using ReSys.Core.Features.Shared.Identity.Admin.Users.UpdateAdminUser;
using ReSys.Core.Features.Shared.Identity.Admin.Users.DeleteAdminUser;
using ReSys.Core.Features.Shared.Identity.Admin.Users.AssignRoleToUser;
using ReSys.Core.Features.Shared.Identity.Admin.Users.UnassignRoleFromUser;
using ReSys.Core.Features.Shared.Identity.Admin.Users.UpdateUserStatus;
using ReSys.Core.Features.Shared.Identity.Admin.Users.AssignPermissionToUser;
using ReSys.Core.Features.Shared.Identity.Admin.Users.UnassignPermissionFromUser;
using ReSys.Core.Features.Shared.Identity.Admin.Users.GetUserPermissions;
using ReSys.Core.Features.Shared.Identity.Admin.Users.GetUserRoles;
using ReSys.Core.Features.Shared.Identity.Admin.Users.SyncUserRoles;
using ReSys.Core.Features.Shared.Identity.Admin.Users.UpdateStaffProfile;
using ReSys.Infrastructure.Security.Authorization;
using ReSys.Shared.Constants.Permissions;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Identity.Admin;

public sealed class AdminUserModule : ICarterModule
{
    private const string BaseRoute = "api/admin/identity/users";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Identity - Admin Users")
            .RequireAuthorization();

        // 1. CRUD Operations
        group.MapGet("/", GetUsersHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.List);
        group.MapGet("{id}", GetUserDetailHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.View);
        group.MapPost("/", CreateUserHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.Create);
        group.MapPut("{id}", UpdateUserHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.Update);
        group.MapDelete("{id}", DeleteUserHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.Delete);
        
        // Status Management
        group.MapPatch("{id}/status", UpdateUserStatusHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.Update);
        group.MapPut("{id}/staff-profile", UpdateStaffProfileHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.Update);
        
        // Role Management
        group.MapGet("{id}/roles", GetUserRolesHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.View);
        group.MapPost("{id}/roles", AssignRoleHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.AssignRole);
        group.MapPut("{id}/roles", SyncRolesHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.AssignRole);
        group.MapDelete("{id}/roles/{roleName}", UnassignRoleHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.UnassignRole);

        // 4. Direct Permission Management
        group.MapGet("{id}/permissions", GetUserPermissionsHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.AccessControl.View);
        group.MapPost("{id}/permissions", AssignPermissionHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.AssignRole);
        group.MapDelete("{id}/permissions/{permissionName}", UnassignPermissionHandler).RequireAccessPermission(FeaturePermissions.Admin.Identity.User.UnassignRole);
    }

    private static async Task<IResult> GetUsersHandler([AsParameters] GetAdminUsersPagedList.Request options, ISender mediator)
    {
        var result = await mediator.Send(new GetAdminUsersPagedList.Query(options));
        return result.ToTypedApiResponse("Admin users list retrieved");
    }

    private static async Task<IResult> GetUserDetailHandler([FromRoute] string id, ISender mediator)
    {
        var result = await mediator.Send(new GetAdminUserDetail.Query(id));
        return result.ToTypedApiResponse("User details retrieved");
    }

    private static async Task<IResult> CreateUserHandler([FromBody] CreateAdminUser.Request request, ISender mediator)
    {
        var result = await mediator.Send(new CreateAdminUser.Command(request));
        return result.ToTypedApiResponse("User created successfully");
    }

    private static async Task<IResult> UpdateUserHandler([FromRoute] string id, [FromBody] UpdateAdminUser.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateAdminUser.Command(id, request));
        return result.ToTypedApiResponse("User updated successfully");
    }

    private static async Task<IResult> DeleteUserHandler([FromRoute] string id, ISender mediator)
    {
        var result = await mediator.Send(new DeleteAdminUser.Command(id));
        return result.ToTypedApiResponse("User deleted successfully");
    }

    private static async Task<IResult> UpdateUserStatusHandler([FromRoute] string id, [FromBody] UpdateUserStatus.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateUserStatus.Command(id, request));
        return result.ToTypedApiResponse("User status updated");
    }

    private static async Task<IResult> UpdateStaffProfileHandler([FromRoute] string id, [FromBody] UpdateStaffProfile.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateStaffProfile.Command(id, request));
        return result.ToTypedApiResponse("Staff profile updated");
    }

    private static async Task<IResult> GetUserRolesHandler([FromRoute] string id, ISender mediator)
    {
        var result = await mediator.Send(new GetUserRoles.Query(id));
        return result.ToTypedApiResponse("User roles retrieved");
    }

    private static async Task<IResult> AssignRoleHandler([FromRoute] string id, [FromBody] AssignRoleToUser.Request request, ISender mediator)
    {
        var result = await mediator.Send(new AssignRoleToUser.Command(id, request));
        return result.ToTypedApiResponse("Role assigned to user");
    }

    private static async Task<IResult> UnassignRoleHandler([FromRoute] string id, [FromRoute] string roleName, ISender mediator)
    {
        var result = await mediator.Send(new UnassignRoleFromUser.Command(id, roleName));
        return result.ToTypedApiResponse("Role unassigned from user");
    }

    private static async Task<IResult> SyncRolesHandler([FromRoute] string id, [FromBody] SyncUserRoles.Request request, ISender mediator)
    {
        var result = await mediator.Send(new SyncUserRoles.Command(id, request));
        return result.ToTypedApiResponse("User roles synchronized");
    }

    private static async Task<IResult> GetUserPermissionsHandler([FromRoute] string id, ISender mediator)
    {
        var result = await mediator.Send(new GetUserPermissions.Query(id));
        return result.ToTypedApiResponse("User effective permissions retrieved");
    }

    private static async Task<IResult> AssignPermissionHandler([FromRoute] string id, [FromBody] AssignPermissionToUser.Request request, ISender mediator)
    {
        var result = await mediator.Send(new AssignPermissionToUser.Command(id, request));
        return result.ToTypedApiResponse("Direct permission assigned to user");
    }

    private static async Task<IResult> UnassignPermissionHandler([FromRoute] string id, [FromRoute] string permissionName, ISender mediator)
    {
        var result = await mediator.Send(new UnassignPermissionFromUser.Command(id, permissionName));
        return result.ToTypedApiResponse("Direct permission unassigned from user");
    }
}
