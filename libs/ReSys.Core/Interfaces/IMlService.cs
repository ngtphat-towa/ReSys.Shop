namespace ReSys.Core.Interfaces;

public interface IMlService
{
    Task<float[]?> GetEmbeddingAsync(string imageUrl, string ExampleId, CancellationToken cancellationToken = default);
}
