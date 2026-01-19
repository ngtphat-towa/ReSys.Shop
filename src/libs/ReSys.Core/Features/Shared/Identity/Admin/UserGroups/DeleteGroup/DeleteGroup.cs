using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.UserGroups;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Admin.UserGroups.DeleteGroup;

public static class DeleteGroup
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            var group = await context.Set<UserGroup>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

            if (group is null) return UserGroupErrors.NotFound(command.Id);

            var deleteResult = group.Delete();
            if (deleteResult.IsError) return deleteResult.Errors;

            context.Set<UserGroup>().Update(group);
            await context.SaveChangesAsync(ct);

            return Result.Deleted;
        }
    }
}
