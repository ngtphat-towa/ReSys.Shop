namespace ReSys.Core.Common.AI;

public interface IMlService
{
    Task<float[]?> GetEmbeddingAsync(string imageUrl, string productId, CancellationToken cancellationToken = default);
}
