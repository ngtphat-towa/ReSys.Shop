using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Ml;

namespace ReSys.Infrastructure.Ml.Services;

public class FakeMlService(ILogger<FakeMlService> logger) : IMlService
{
    public Task<float[]?> GetEmbeddingAsync(string imageUrl, string exampleId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("ML: [FAKE] Generating embedding for image {ImageUrl} and example {ExampleId}", imageUrl, exampleId);
        
        // Return a dummy vector (length 3 to match pgvector simple tests)
        return Task.FromResult<float[]?>([0.1f, 0.2f, 0.3f]);
    }
}