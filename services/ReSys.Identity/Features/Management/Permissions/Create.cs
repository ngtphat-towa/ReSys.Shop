using ErrorOr;
using FluentValidation;
using MediatR;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence;

namespace ReSys.Identity.Features.Management.Permissions;

public static class Create
{
    public record Request(string Type, string Value, string Description, string Category);

    public record Command(Request Data) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Data.Type).NotEmpty();
            RuleFor(x => x.Data.Value).NotEmpty();
        }
    }

    public class Handler(AppIdentityDbContext dbContext) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
        {
            var exists = dbContext.ClaimDefinitions.Any(x => x.Type == request.Data.Type && x.Value == request.Data.Value);
            if (exists) return Error.Conflict("Permissions.Duplicate", "Permission already exists.");

            var entity = new ClaimDefinition
            {
                Type = request.Data.Type,
                Value = request.Data.Value,
                Description = request.Data.Description,
                Category = request.Data.Category
            };

            dbContext.ClaimDefinitions.Add(entity);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
