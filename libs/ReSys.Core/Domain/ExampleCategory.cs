namespace ReSys.Core.Domain;

public class ExampleCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual ICollection<Example> Examples { get; set; } = new List<Example>();
}
