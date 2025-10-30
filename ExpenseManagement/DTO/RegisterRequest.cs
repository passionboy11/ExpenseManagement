using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTO;

public class RegisterRequest
{
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
    [Required]
    public required string Role { get; set; }
}