using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTO;

public class EditBudget
{
    [Required]
    [MaxLength(50)]
    public string Category { get; set; }
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Limit amount must be greater than 0")]
    public decimal LimitAmount { get; set; }

    [Required]
    [RegularExpression(@"^(0[1-9]|1[0-2])-\d{4}$", ErrorMessage = "MonthYear must be in MM-yyyy format")]
    public string MonthYear { get; set; } 
}
