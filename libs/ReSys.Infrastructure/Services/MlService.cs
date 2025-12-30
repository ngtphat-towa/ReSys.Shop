using System.Net.Http.Json;
using ReSys.Core.Interfaces;

namespace ReSys.Infrastructure.Services;

public class MlService : IMlService
{
    private readonly HttpClient _httpClient;

    public MlService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("MlService");
    }

    public async Task<float[]?> GetEmbeddingAsync(string imageUrl, string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var mlRequest = new { image_url = imageUrl, product_id = productId };
            var response = await _httpClient.PostAsJsonAsync("/embed", mlRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<MLResponse>(cancellationToken: cancellationToken);
                return result?.Embedding;
            }
        }
        catch (Exception) { }

        return null;
    }

    private class MLResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }
    }
}
