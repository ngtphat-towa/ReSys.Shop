using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Testing.ExampleCategories;
using ReSys.Core.Features.Testing.ExampleCategories.Common;

namespace ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategoryById;

public static class GetExampleCategoryById
{
    public record Query(Guid Id) : IRequest<ErrorOr<ExampleCategoryDetail>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<ExampleCategoryDetail>>
    {
        public async Task<ErrorOr<ExampleCategoryDetail>> Handle(Query query, CancellationToken cancellationToken)
        {
            var category = await context.Set<ExampleCategory>()
                .AsNoTracking()
                .Select(ExampleCategoryDetail.Projection)
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (category == null)
            {
                return ExampleCategoryErrors.NotFound;
            }

            return category;
        }
    }
}
