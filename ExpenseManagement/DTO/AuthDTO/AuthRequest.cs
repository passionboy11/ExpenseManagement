using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTO;

public class AuthRequest
{
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
}