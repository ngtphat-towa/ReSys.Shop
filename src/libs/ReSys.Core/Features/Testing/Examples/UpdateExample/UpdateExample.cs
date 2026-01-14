using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Common.Storage;
using ReSys.Core.Domain.Testing.Examples;

namespace ReSys.Core.Features.Testing.Examples.UpdateExample;

public static class UpdateExample
{
    public record Request : ExampleInput;

    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<ExampleDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }

        private class RequestValidator : ExampleValidator<Request> { }
    }

    public class Handler(IApplicationDbContext context, IFileService fileService) : IRequestHandler<Command, ErrorOr<ExampleDetail>>
    {
        public async Task<ErrorOr<ExampleDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var example = await context.Set<Example>().FindAsync(new object[] { command.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(command.Id);
            }

            var request = command.Request;

            if (await context.Set<Example>().AnyAsync(x => x.Name == request.Name && x.Id != command.Id, cancellationToken))
            {
                return ExampleErrors.DuplicateName;
            }

            example.Name = request.Name;
            example.Description = request.Description;
            example.Price = request.Price;
            example.Status = request.Status;
            example.HexColor = request.HexColor;
            example.CategoryId = request.CategoryId;

            if (!string.IsNullOrEmpty(request.ImageUrl) && example.ImageUrl != request.ImageUrl)
            {
                // Delete old image if it's different
                if (!string.IsNullOrEmpty(example.ImageUrl))
                {
                    var oldFileId = example.ImageUrl.Replace("/api/files/", "");
                    if (!string.IsNullOrEmpty(oldFileId))
                    {
                        await fileService.DeleteFileAsync(oldFileId, cancellationToken);
                    }
                }
                example.ImageUrl = request.ImageUrl;
            }

            await context.SaveChangesAsync(cancellationToken);

            return await context.Set<Example>()
                .AsNoTracking()
                .Select(ExampleDetail.Projection)
                .FirstAsync(x => x.Id == example.Id, cancellationToken);
        }
    }
}
