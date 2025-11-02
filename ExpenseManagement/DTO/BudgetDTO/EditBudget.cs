namespace ExpenseManagement.DTO;

public class EditBudget
{
    public int Id { get; set; }
    public string Category { get; set; }
    public decimal LimitAmount { get; set; }
    public string MonthYear { get; set; }
}