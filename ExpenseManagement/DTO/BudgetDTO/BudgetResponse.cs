namespace ExpenseManagement.DTO;

public class BudgetResponse
{
    public string Category { get; set; }
    public decimal LimitAmount { get; set; }
    public string MonthYear { get; set; }
}