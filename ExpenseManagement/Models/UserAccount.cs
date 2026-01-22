using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models;

public class UserAccount
{
    [Required]
    public int Id { get; set; }      
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public string Role { get; set; } = "user";
}