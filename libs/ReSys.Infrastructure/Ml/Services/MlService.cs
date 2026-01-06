using System.Net.Http.Json;

using Microsoft.Extensions.Options;

using ReSys.Core.Common.Ml;
using ReSys.Infrastructure.Ml.Options;

namespace ReSys.Infrastructure.Ml.Services;

public class MlService : IMlService
{
    private readonly HttpClient _httpClient;

    public MlService(HttpClient httpClient, IOptions<MlOptions> options)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(options.Value.ServiceUrl);
    }

    public async Task<float[]?> GetEmbeddingAsync(string imageUrl, string ExampleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var mlRequest = new { image_url = imageUrl, Example_id = ExampleId };
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
