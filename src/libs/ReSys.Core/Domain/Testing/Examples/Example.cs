using System.Text.Json.Serialization;

using ReSys.Core.Domain.Common.Abtractions;
using ReSys.Core.Domain.Testing.ExampleCategories;

namespace ReSys.Core.Domain.Testing.Examples;

public enum ExampleStatus
{
    Draft,
    Active,
    Archived
}

public class Example : Entity, IAuditable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public ExampleStatus Status { get; set; } = ExampleStatus.Draft;
    public string? HexColor { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CategoryId { get; set; }
    public virtual ExampleCategory? Category { get; set; }

    public virtual ExampleEmbedding? Embedding { get; set; }
}

public class ExampleEmbedding
{
    public Guid ExampleId { get; set; }
    public Pgvector.Vector Embedding { get; set; } = null!;

    [JsonIgnore]
    public virtual Example Example { get; set; } = null!;
}