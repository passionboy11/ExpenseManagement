using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTO;

public class DeleteBudget
{
    [Required]
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
}