using System.Text.Json.Serialization;

namespace ReSys.Core.Domain;

public class Example : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual ExampleEmbedding? Embedding { get; set; }
}

public class ExampleEmbedding
{
    public Guid ExampleId { get; set; }
    public Pgvector.Vector Embedding { get; set; } = null!;
    
    [JsonIgnore]
    public virtual Example Example { get; set; } = null!;
}