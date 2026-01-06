using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Common.Storage;

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

    public class Handler : IRequestHandler<Command, ErrorOr<ExampleDetail>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileService _fileService;

        public Handler(IApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<ErrorOr<ExampleDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var example = await _context.Set<Example>().FindAsync(new object[] { command.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(command.Id);
            }

            var request = command.Request;

            if (await _context.Set<Example>().AnyAsync(x => x.Name == request.Name && x.Id != command.Id, cancellationToken))
            {
                return ExampleErrors.DuplicateName;
            }

            example.Name = request.Name;
            example.Description = request.Description;
            example.Price = request.Price;
            example.Status = request.Status;
            example.HexColor = request.HexColor;
            
            if (!string.IsNullOrEmpty(request.ImageUrl) && example.ImageUrl != request.ImageUrl)
            {
                // Delete old image if it's different
                if (!string.IsNullOrEmpty(example.ImageUrl))
                {
                    var oldFileId = example.ImageUrl.Replace("/api/files/", "");
                    if (!string.IsNullOrEmpty(oldFileId))
                    {
                        await _fileService.DeleteFileAsync(oldFileId, cancellationToken);
                    }
                }
                example.ImageUrl = request.ImageUrl;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new ExampleDetail
            {
                Id = example.Id,
                Name = example.Name,
                Description = example.Description,
                Price = example.Price,
                ImageUrl = example.ImageUrl,
                Status = example.Status,
                HexColor = example.HexColor,
                CreatedAt = example.CreatedAt
            };
        }
    }
}
