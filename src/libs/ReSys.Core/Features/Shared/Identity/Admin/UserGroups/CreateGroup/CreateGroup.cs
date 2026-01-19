using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.UserGroups;
using ReSys.Core.Features.Shared.Identity.Admin.UserGroups.Common;
using ErrorOr;
using FluentValidation;
using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Admin.UserGroups.CreateGroup;

public static class CreateGroup
{
    public record Request : GroupParameters;
    public record Response : GroupResponse;
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(UserGroupConstraints.NameMaxLength);
            RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(UserGroupConstraints.CodeMaxLength);
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var req = command.Request;

            // Guard: Unique Code
            if (await context.Set<UserGroup>().AnyAsync(x => x.Code == req.Code.ToUpperInvariant(), ct))
                return UserGroupErrors.DuplicateCode(req.Code);

            // Create Aggregate
            var groupResult = UserGroup.Create(req.Name, req.Code, req.Description, isDefault: req.IsDefault);
            if (groupResult.IsError) return groupResult.Errors;

            var group = groupResult.Value;
            context.Set<UserGroup>().Add(group);
            await context.SaveChangesAsync(ct);

            return group.Adapt<Response>();
        }
    }
}
