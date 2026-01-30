using ExpenseManagement.Models.Enums;

namespace ExpenseManagement.DTO.ReminderDTO;

public class EditReminderRequest
{
    
    public string BillName { get; set; }
    public DateTime DueDate { get; set; }
    public string PaymentMethod { get; set; }
    public  ReminderFrequency Frequency { get; set; }
    public ReminderStatus Status { get; set; }
    public int NotificationTiming { get; set; }
}