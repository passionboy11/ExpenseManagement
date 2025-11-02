namespace ExpenseManagement.DTO;

public class TransactionResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PaymentMethod { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime Date { get; set; }
}