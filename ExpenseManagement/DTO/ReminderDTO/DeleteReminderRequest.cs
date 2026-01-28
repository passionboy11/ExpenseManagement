using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.DTO.ReminderDTO;

public class DeleteReminderRequest
{
    [Required]
    public int UserId { get; set; }
    public int Id { get; set; }
}