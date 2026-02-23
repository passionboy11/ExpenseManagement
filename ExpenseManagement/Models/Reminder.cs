using ExpenseManagement.Models.Enums;

namespace ExpenseManagement.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        
        public string BillName { get; set; } = null!;
        
        public DateTime DueDate { get; set; }
        
        public PaymentMethod PaymentMethod { get; set; } 
        
        public ReminderFrequency Frequency { get; set; }
        
        public int NotificationTiming { get; set; }
  
        public ReminderStatus Status { get; set; }
    }
}
