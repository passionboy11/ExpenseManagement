using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTO;

public class EditTransactionRequest
{
   
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public bool IsExpense { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters.")]
    public string Category { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
    public string? Description { get; set; }

    [MaxLength(50, ErrorMessage = "PaymentMethod cannot exceed 50 characters.")]
    public string? PaymentMethod { get; set; }

    public bool IsRecurring { get; set; } = false;

    [Required(ErrorMessage = "Date is required.")]
    [DataType(DataType.DateTime)]
    public DateTime Date { get; set; } = DateTime.UtcNow;
}