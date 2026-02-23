using System.Text.Json.Serialization;

namespace ExpenseManagement.DTO;

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string? RefreshToken { get; set; }
    [JsonIgnore]
    public DateTime? Expires { get; set; }
}