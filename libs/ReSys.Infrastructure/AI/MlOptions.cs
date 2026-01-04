namespace ReSys.Infrastructure.AI;

public class MlOptions
{
    public const string SectionName = "MlSettings";
    
    public string ServiceUrl { get; set; } = "http://localhost:8000";
}
