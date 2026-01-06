using System.Text.Json.Serialization;

namespace ReSys.Core.Features.Identity.Common;

public record UserBase
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    [JsonPropertyName("user_type")]
    public string UserType { get; set; } = string.Empty;
}

public record UserListItem : UserBase
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public record UserDetail : UserListItem
{
    [JsonPropertyName("roles")]
    public IEnumerable<string> Roles { get; set; } = [];
}
