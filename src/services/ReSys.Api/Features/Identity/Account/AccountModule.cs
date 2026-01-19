using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Identity.Account.Profile;
using ReSys.Core.Features.Identity.Account.Addresses;
using ReSys.Core.Features.Identity.Account.Communication;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Identity.Account;

public sealed class AccountModule : ICarterModule
{
    private const string BaseRoute = "api/account";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BaseRoute)
            .WithTags("Identity - Account")
            .RequireAuthorization();

        // 1. Profile
        group.MapGet("/profile", GetProfileHandler);
        group.MapPut("/profile", UpdateProfileHandler);

        // 2. Addresses
        var addressGroup = group.MapGroup("/addresses");
        addressGroup.MapGet("/", GetAddressesHandler);
        addressGroup.MapGet("/select", GetAddressSelectHandler); // Selection utility
        addressGroup.MapPost("/", CreateAddressHandler);
        addressGroup.MapPut("/{id:guid}", UpdateAddressHandler);
        addressGroup.MapDelete("/{id:guid}", DeleteAddressHandler);

        // 3. Communication
        group.MapPost("/email/change", ChangeEmailHandler);
        group.MapPost("/phone/change", ChangePhoneHandler);
    }

    private static async Task<IResult> GetProfileHandler(ISender mediator)
    {
        var result = await mediator.Send(new GetProfile.Query());
        return result.ToTypedApiResponse("Profile retrieved");
    }

    private static async Task<IResult> UpdateProfileHandler([FromBody] UpdateProfile.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateProfile.Command(request));
        return result.ToTypedApiResponse("Profile updated");
    }

    private static async Task<IResult> GetAddressesHandler([AsParameters] GetAddressesPagedList.Request options, ISender mediator)
    {
        var result = await mediator.Send(new GetAddressesPagedList.Query(options));
        return result.ToTypedApiResponse("Addresses retrieved");
    }

    private static async Task<IResult> GetAddressSelectHandler([AsParameters] QueryOptions options, ISender mediator)
    {
        var result = await mediator.Send(new GetAddressSelectList.Query(options));
        return result.ToTypedApiResponse("Address selection list retrieved");
    }

    private static async Task<IResult> CreateAddressHandler([FromBody] CreateAddress.Request request, ISender mediator)
    {
        var result = await mediator.Send(new CreateAddress.Command(request));
        return result.ToTypedApiResponse("Address created");
    }

    private static async Task<IResult> UpdateAddressHandler([FromRoute] Guid id, [FromBody] UpdateAddress.Request request, ISender mediator)
    {
        var result = await mediator.Send(new UpdateAddress.Command(id, request));
        return result.ToTypedApiResponse("Address updated");
    }

    private static async Task<IResult> DeleteAddressHandler([FromRoute] Guid id, ISender mediator)
    {
        var result = await mediator.Send(new DeleteAddress.Command(id));
        return result.ToTypedApiResponse("Address deleted");
    }

    private static async Task<IResult> ChangeEmailHandler([FromBody] ChangeEmail.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ChangeEmail.Command(request));
        return result.ToTypedApiResponse();
    }

    private static async Task<IResult> ChangePhoneHandler([FromBody] ChangePhone.Request request, ISender mediator)
    {
        var result = await mediator.Send(new ChangePhone.Command(request));
        return result.ToTypedApiResponse();
    }
}