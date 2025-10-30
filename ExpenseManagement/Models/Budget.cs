namespace ExpenseManagement.Models;

public class Budget
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Category { get; set; }
    public decimal LimitAmount { get; set; }
    public string MonthYear { get; set; }
    public DateTime CreatedAt { get; set; }
}