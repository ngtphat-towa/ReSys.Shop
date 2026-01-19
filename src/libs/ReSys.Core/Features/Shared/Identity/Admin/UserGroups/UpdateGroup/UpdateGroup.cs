using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.UserGroups;
using ReSys.Core.Features.Shared.Identity.Admin.UserGroups.Common;
using ErrorOr;
using FluentValidation;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Admin.UserGroups.UpdateGroup;

public static class UpdateGroup
{
    public record Request
    {
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public bool IsDefault { get; init; }
    }

    public record Response : GroupResponse;
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(UserGroupConstraints.NameMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var group = await context.Set<UserGroup>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (group is null) return UserGroupErrors.NotFound(command.Id);

            var req = command.Request;
            var updateResult = group.Update(req.Name, req.Description, req.IsDefault);
            
            if (updateResult.IsError) return updateResult.Errors;

            context.Set<UserGroup>().Update(group);
            await context.SaveChangesAsync(ct);

            return group.Adapt<Response>();
        }
    }
}
