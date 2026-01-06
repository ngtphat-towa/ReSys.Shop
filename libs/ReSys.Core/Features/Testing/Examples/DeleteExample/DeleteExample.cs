using ErrorOr;
using MediatR;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Common.Storage;

namespace ReSys.Core.Features.Testing.Examples.DeleteExample;

public static class DeleteExample
{
    public class Command(Guid id) : IRequest<ErrorOr<Deleted>>
    {
        public Guid Id { get; set; } = id;
    }

    public class Handler(IApplicationDbContext context, IFileService fileService) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        private readonly IApplicationDbContext _context = context;
        private readonly IFileService _fileService = fileService;

        public async Task<ErrorOr<Deleted>> Handle(Command request, CancellationToken cancellationToken)
        {
            var example = await _context.Set<Example>().FindAsync(new object[] { request.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(request.Id);
            }

            // Delete associated image if exists
            if (!string.IsNullOrEmpty(example.ImageUrl))
            {
                var fileId = example.ImageUrl.Replace("/api/files/", "");
                if (!string.IsNullOrEmpty(fileId))
                {
                    await _fileService.DeleteFileAsync(fileId, cancellationToken);
                }
            }

            _context.Set<Example>().Remove(example);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}