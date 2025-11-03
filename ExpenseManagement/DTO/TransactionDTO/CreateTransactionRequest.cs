using Newtonsoft.Json;

namespace ExpenseManagement.DTO;

public class CreateTransactionRequest
{
    public decimal Amount { get; set; }
    public bool IsExpense { get; set; } 
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PaymentMethod { get; set; }
    public bool IsRecurring { get; set; } = false;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}