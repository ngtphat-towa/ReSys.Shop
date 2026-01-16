namespace ReSys.Core.Common.Ml;

public interface IMlService
{
    Task<float[]?> GetEmbeddingAsync(string imageUrl, string productId, CancellationToken cancellationToken = default);
}
