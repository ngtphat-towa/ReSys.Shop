namespace ReSys.Infrastructure.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string LocalPath { get; set; } = "storage";
}
