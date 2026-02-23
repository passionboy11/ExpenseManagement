using System.ComponentModel.DataAnnotations;
using ExpenseManagement.Models.Enums;

namespace ExpenseManagement.DTO.ReminderDTO;

public class EditReminderRequest
{
    [Required]
    [MaxLength(100)]
    public string BillName { get; set; }
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }
    [Required]
    [EnumDataType(typeof(PaymentMethod),ErrorMessage = "Invalid payment method.")]
    public PaymentMethod PaymentMethod { get; set; }
    [Required]
    [EnumDataType(typeof(ReminderFrequency), ErrorMessage = "Invalid frequency.")]
    public ReminderFrequency Frequency { get; set; }
    [Required]
    public int NotificationTiming { get; set; }
    [Required]
    [EnumDataType(typeof(ReminderStatus), ErrorMessage = "Invalid status.")]
    public ReminderStatus Status { get; set; }
}