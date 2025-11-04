namespace ExpenseManagement.DTO;

public class BudgetResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public string Category { get; set; }
    public decimal LimitAmount { get; set; }
    public string MonthYear { get; set; }
}