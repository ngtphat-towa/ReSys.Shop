using System.Text.Json.Serialization;

namespace ReSys.Core.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual ProductEmbedding? Embedding { get; set; }
}

public class ProductEmbedding
{
    public Guid ProductId { get; set; }
    public Pgvector.Vector Embedding { get; set; } = null!;
    
    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;
}