using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReSys.Core.Features.Admin.Settings.PaymentMethods;
using ReSys.Core.Features.Admin.Settings.PaymentMethods.Common;
using ReSys.Shared.Models.Wrappers;

namespace ReSys.Api.Features.Admin.Settings;

public class PaymentMethodEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/admin/settings/payment-methods")
            .WithTags("Admin.Settings.PaymentMethods")
            .RequireAuthorization();

        group.MapGet("", async (ISender sender) =>
        {
            var result = await sender.Send(new GetPaymentMethods.Query());
            return result.ToTypedApiResponse();
        });

        group.MapGet("{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetPaymentMethodById.Query(id));
            return result.ToTypedApiResponse();
        });

        group.MapPost("", async (PaymentMethodInput request, ISender sender) =>
        {
            var result = await sender.Send(new CreatePaymentMethod.Command(request));
            return result.ToTypedApiResponse("Payment method created");
        });

        group.MapPut("{id}", async (Guid id, PaymentMethodInput request, ISender sender) =>
        {
            var result = await sender.Send(new UpdatePaymentMethod.Command(id, request));
            return result.ToTypedApiResponse("Payment method updated");
        });

        group.MapDelete("{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeletePaymentMethod.Command(id));
            return result.ToTypedApiResponse("Payment method deleted");
        });

        group.MapPatch("{id}/status", async (Guid id, [FromBody] bool isActive, ISender sender) =>
        {
            var result = await sender.Send(new UpdatePaymentMethodStatus.Command(id, isActive));
            return result.ToTypedApiResponse("Status updated");
        });

        group.MapPut("{id}/config", async (Guid id, [FromBody] Dictionary<string, string> secrets, ISender sender) =>
        {
            var command = new UpdatePaymentMethodConfig.Command(id, secrets);
            var result = await sender.Send(command);
            return result.ToTypedApiResponse("Configuration updated");
        });
    }
}