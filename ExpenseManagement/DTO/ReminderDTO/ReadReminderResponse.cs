using ExpenseManagement.Models.Enums;

namespace ExpenseManagement.DTO.ReminderDTO;

public class ReadReminderResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BillName { get; set; }
    public DateTime DueDate { get; set; }
    public string PaymentMethod { get; set; }
    public ReminderFrequency Frequency { get; set; }
    public int NotificationTiming { get; set; }
    public ReminderStatus Status { get; set; }
}