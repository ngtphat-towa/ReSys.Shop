using System.ComponentModel.DataAnnotations;

namespace ReSys.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    [Required, MinLength(1)]
    public string LocalPath { get; set; } = "storage";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public int BufferSize { get; set; } = 81920;
    
    [Required]
    public SecurityOptions Security { get; set; } = new();
}

public sealed class SecurityOptions
{
    [Required, MinLength(8)]
    public string EncryptionKey { get; set; } = string.Empty;
    public string[] DangerousExtensions { get; set; } = new[]
    {
        ".exe", ".dll", ".bat", ".cmd", ".com", ".scr", 
        ".vbs", ".js", ".jar", ".msi", ".sh", ".ps1"
    };
    public bool ValidateFileSignatures { get; set; } = true;
}
