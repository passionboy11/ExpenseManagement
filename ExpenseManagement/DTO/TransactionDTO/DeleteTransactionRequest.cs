using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTO;

public class DeleteTransactionRequest
{
    [Required]
    public int UserId { get; set; }
    [Required]
    public int Id { get; set; }
}