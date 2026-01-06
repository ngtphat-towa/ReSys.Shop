using System.Text.Json.Serialization;

namespace ReSys.Core.Features.Identity.Common;

public record RoleBase
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public record RoleListItem : RoleBase
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public record RoleDetail : RoleListItem
{
    [JsonPropertyName("permissions")]
    public IEnumerable<string> Permissions { get; set; } = [];
}
