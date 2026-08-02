using System.Text.Json.Serialization;

namespace ExpenseManagement.DTO;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;

    // Never sent to the client in the JSON body - it's already delivered as an
    // httpOnly cookie. Serializing it here too would let any injected JS read it,
    // defeating the point of httpOnly.
    [JsonIgnore]
    public string? RefreshToken { get; set; }
    [JsonIgnore]
    public DateTime? Expires { get; set; }
}