using System.Text.Json.Serialization;
using ReSys.Core.Domain.Abstractions.Concerns;

namespace ReSys.Core.Domain;

public enum ExampleStatus
{
    Draft,
    Active,
    Archived
}

public class Example : Entity, IAuditable, ISoftDeletable, IVersioned
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public ExampleStatus Status { get; set; } = ExampleStatus.Draft;
    public string? HexColor { get; set; }
    
    // IAuditable
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // IVersioned
    public int Version { get; set; }

    public virtual ExampleEmbedding? Embedding { get; set; }

    public static Example Create(string name, decimal price)
    {
        var example = new Example
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Status = ExampleStatus.Draft
        };

        return example;
    }
}

public class ExampleEmbedding
{
    public Guid ExampleId { get; set; }
    public Pgvector.Vector Embedding { get; set; } = null!;
    
    [JsonIgnore]
    public virtual Example Example { get; set; } = null!;
}