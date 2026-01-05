using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Identity;
using ReSys.Identity.Features.Account.Contracts;

namespace ReSys.Identity.Features.Account;

public class UsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
                       .WithTags("Users")
                       .RequireAuthorization(); // TODO: Add "Permissions.Users.Manage" policy later

        group.MapGet("/", GetUsers);
        group.MapGet("/{id}", GetUserById);
        group.MapPost("/", CreateUser);
        group.MapPut("/{id}", UpdateUser);
        group.MapDelete("/{id}", DeleteUser);
        group.MapPost("/{id}/roles", AssignRoles);
    }

    private async Task<IResult> GetUsers(UserManager<ApplicationUser> userManager)
    {
        var users = await userManager.Users.ToListAsync();
        var response = users.Select(u => new UserResponse(
            u.Id, 
            u.UserName ?? "", 
            u.Email ?? "", 
            u.FirstName ?? "", 
            u.LastName ?? "", 
            u.UserType.ToString(),
            Array.Empty<string>())); // TODO: Include roles if needed
            
        return Results.Ok(response);
    }

    private async Task<IResult> GetUserById(string id, UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return Results.NotFound();
        
        var roles = await userManager.GetRolesAsync(user);
        
        return Results.Ok(new UserResponse(
            user.Id, 
            user.UserName ?? "", 
            user.Email ?? "", 
            user.FirstName ?? "", 
            user.LastName ?? "", 
            user.UserType.ToString(),
            roles));
    }

    private async Task<IResult> DeleteUser(string id, UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return Results.NotFound();

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded) return Results.BadRequest(result.Errors.Select(e => e.Description));

        return Results.NoContent();
    }

    private async Task<IResult> AssignRoles(
        string id, 
        [FromBody] AssignRolesRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return Results.NotFound();

        var currentRoles = await userManager.GetRolesAsync(user);
        
        // Add new roles
        var rolesToAdd = request.RoleNames.Except(currentRoles).ToList();
        if (rolesToAdd.Any())
        {
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded) return Results.BadRequest(addResult.Errors.Select(e => e.Description));
        }

        // Remove old roles (optional: if AssignRoles implies SET vs ADD. Usually SET is safer for admin UIs)
        var rolesToRemove = currentRoles.Except(request.RoleNames).ToList();
        if (rolesToRemove.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded) return Results.BadRequest(removeResult.Errors.Select(e => e.Description));
        }

        return Results.NoContent();
    }

    private async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request, 
        UserManager<ApplicationUser> userManager,
        IValidator<CreateUserRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.ToDictionary());
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserType = request.UserType,
            EmailConfirmed = true // Auto-confirm for now
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Errors.Select(e => e.Description));
        }

        return Results.Created($"/api/users/{user.Id}", new { user.Id });
    }
    
    private async Task<IResult> UpdateUser(
        string id,
        [FromBody] UpdateUserRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return Results.NotFound();

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return Results.BadRequest(result.Errors.Select(e => e.Description));
        
        return Results.NoContent();
    }
}