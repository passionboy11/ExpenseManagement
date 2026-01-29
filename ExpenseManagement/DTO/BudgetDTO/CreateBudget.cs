
using System.Text.Json.Serialization;

namespace ExpenseManagement.DTO;

public class CreateBudget
{
    public string Category { get; set; }
    public decimal LimitAmount { get; set; }
    public string MonthYear { get; set; }
}