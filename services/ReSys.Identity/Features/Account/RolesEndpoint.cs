using System.Security.Claims;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Identity;
using ReSys.Identity.Features.Account.Contracts;

namespace ReSys.Identity.Features.Account;

public class RolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
                       .WithTags("Roles")
                       .RequireAuthorization();

        group.MapGet("/", GetRoles);
        group.MapGet("/{id}", GetRoleById);
        group.MapPost("/", CreateRole);
        group.MapPut("/{id}/permissions", UpdateRolePermissions);
    }

    private async Task<IResult> GetRoles(RoleManager<ApplicationRole> roleManager)
    {
        var roles = await roleManager.Roles.ToListAsync();
        return Results.Ok(roles.Select(r => new { r.Id, r.Name }));
    }

    private async Task<IResult> GetRoleById(string id, RoleManager<ApplicationRole> roleManager)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return Results.NotFound();
        
        var claims = await roleManager.GetClaimsAsync(role);
        
        return Results.Ok(new RoleResponse(
             role.Id, 
             role.Name ?? "",
             claims.Where(c => c.Type == "permission").Select(c => c.Value)
        ));
    }

    private async Task<IResult> CreateRole(
        [FromBody] CreateRoleRequest request, 
        RoleManager<ApplicationRole> roleManager,
        IValidator<CreateRoleRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.ToDictionary());
        }

        if (await roleManager.RoleExistsAsync(request.Name))
        {
            return Results.BadRequest("Role already exists.");
        }

        var role = new ApplicationRole(request.Name);
        var result = await roleManager.CreateAsync(role);
        
        if (!result.Succeeded) return Results.BadRequest(result.Errors.Select(e => e.Description));

        return Results.Created($"/api/roles/{role.Id}", new { role.Id });
    }
    
    private async Task<IResult> UpdateRolePermissions(
        string id,
        [FromBody] UpdateRolePermissionsRequest request,
        RoleManager<ApplicationRole> roleManager)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return Results.NotFound();

        // Get existing claims
        var existingClaims = await roleManager.GetClaimsAsync(role);
        var existingPermissions = existingClaims.Where(c => c.Type == "permission").ToList();

        // Determine what to add and remove
        // We assume request.Permissions is the FULL new list
        
        // Remove ones not in new list
        foreach (var claim in existingPermissions)
        {
            if (!request.Permissions.Contains(claim.Value))
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }
        }

        // Add ones not in old list
        foreach (var permission in request.Permissions)
        {
            if (!existingPermissions.Any(c => c.Value == permission))
            {
                await roleManager.AddClaimAsync(role, new Claim("permission", permission));
            }
        }
        
        return Results.NoContent();
    }
}