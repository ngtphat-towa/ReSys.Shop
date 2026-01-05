using System.Security.Claims;
using Carter;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Domain.Identity;
using ReSys.Identity.Features.Account.Contracts;
using ReSys.Core.Common.Mailing;

namespace ReSys.Identity.Features.Account;

public class AccountEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/account")
                       .WithTags("Account");

        // Authenticated routes
        group.MapGet("/profile", GetProfile).RequireAuthorization();
        group.MapPut("/profile", UpdateProfile).RequireAuthorization();
        group.MapPost("/change-password", ChangePassword).RequireAuthorization();
        
        // Public routes
        group.MapPost("/forgot-password", ForgotPassword);
        group.MapPost("/reset-password", ResetPassword);
    }

    private async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        UserManager<ApplicationUser> userManager,
        IMailService mailService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) 
        {
            // Don't reveal user existence
            return Results.Ok();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        // In real app, build a link to Frontend UI with token
        await mailService.SendEmailAsync(request.Email, "Reset Password", $"Token: {token}");
        
        return Results.Ok();
    }

    private async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null) return Results.Ok(); // Don't reveal

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded) return Results.BadRequest(result.Errors.Select(e => e.Description));

        return Results.Ok();
    }

    private async Task<IResult> GetProfile(
        HttpContext httpContext, 
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
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

    private async Task<IResult> UpdateProfile(
        HttpContext httpContext,
        [FromBody] UpdateUserRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user == null) return Results.NotFound();

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return Results.BadRequest(result.Errors.Select(e => e.Description));

        return Results.NoContent();
    }

    private async Task<IResult> ChangePassword(
        HttpContext httpContext,
        [FromBody] ChangePasswordRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user == null) return Results.NotFound();

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded) return Results.BadRequest(result.Errors.Select(e => e.Description));

        return Results.NoContent();
    }
}
