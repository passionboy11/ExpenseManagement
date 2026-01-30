using System;
using System.ComponentModel.DataAnnotations;
using ExpenseManagement.Models.Enums;

namespace ExpenseManagement.Models
{
    public class Reminder
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bill name is required.")]
        [MaxLength(100, ErrorMessage = "Bill name cannot exceed 100 characters.")]
        public string BillName { get; set; } = null!;

        [Required(ErrorMessage = "Due date is required.")]
        [FutureDate(ErrorMessage = "Due date must be in the future.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        [MaxLength(50, ErrorMessage = "Payment method cannot exceed 50 characters.")]
        public string PaymentMethod { get; set; } = null!;

        [Required(ErrorMessage = "Frequency is required.")]
        [EnumDataType(typeof(ReminderFrequency), ErrorMessage = "Invalid frequency.")]
        public ReminderFrequency Frequency { get; set; }

        [Range(0, 1440, ErrorMessage = "Notification timing must be between 0 and 1440 minutes.")]
        public int NotificationTiming { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [EnumDataType(typeof(ReminderStatus), ErrorMessage = "Invalid status.")]
        public ReminderStatus Status { get; set; }
    }
}
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date <= DateTime.Now)
                return new ValidationResult(ErrorMessage ?? "Date must be in the future.");
        }
        return ValidationResult.Success;
    }
}
