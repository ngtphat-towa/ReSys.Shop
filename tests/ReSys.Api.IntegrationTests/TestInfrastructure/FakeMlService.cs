using ReSys.Core.Common.AI;

namespace ReSys.Api.IntegrationTests.TestInfrastructure;

public class FakeMlService : IMlService
{
    public Task<float[]?> GetEmbeddingAsync(string imageUrl, string ExampleId, CancellationToken cancellationToken = default)
    {
        // Return a dummy vector of 384 dimensions (standard BERT/etc size, or pgvector default)
        return Task.FromResult<float[]?>([.. Enumerable.Repeat(0.1f, 384)]);
    }
}
