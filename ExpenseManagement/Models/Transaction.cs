using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseManagement.Models;

public class Transaction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public UserAccount User { get; set; } = null!;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public bool IsExpense { get; set; } 
    [Required]
    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string? PaymentMethod { get; set; }
    public bool IsRecurring { get; set; } = false;

    public string? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}