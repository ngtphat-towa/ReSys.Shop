using System.Text.Json.Serialization;

namespace ReSys.Core.Features.Identity.Contracts;

public record UserResponse(
    [property: JsonPropertyName("id")] string Id, 
    [property: JsonPropertyName("user_name")] string UserName, 
    [property: JsonPropertyName("email")] string Email, 
    [property: JsonPropertyName("first_name")] string FirstName, 
    [property: JsonPropertyName("last_name")] string LastName, 
    [property: JsonPropertyName("user_type")] string UserType,
    [property: JsonPropertyName("roles")] IEnumerable<string> Roles);
