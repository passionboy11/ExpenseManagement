using ExpenseManagement.DTO.ReminderDTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface IReminderService
{
    ReminderServiceResult CreateReminder(CreateReminderRequest request, int userId);
    ReminderServiceResult EditReminder(EditReminderRequest request, int userId, int rid);
    ReminderServiceResult DeleteReminder(int userId, int rid);
    ReminderServiceResult<List<ReminderResponse>> GetReminder(int userId);
    ReminderServiceResult<ReminderResponse> GetReminderById(int userId, int rid);
}

public class ReminderService : IReminderService
{
    private readonly IReminderRepository reminderRepository;
    private readonly ILogger<ReminderService> logger;

    public ReminderService(IReminderRepository reminderRepository, ILogger<ReminderService> logger)
    {
        this.reminderRepository = reminderRepository;
        this.logger = logger;
    }

    public ReminderServiceResult CreateReminder(CreateReminderRequest request, int userId)
    {
        try
        {
            var success = reminderRepository.CreateReminder(request, userId);
            if (!success)
                return new ReminderServiceResult(false, "Failed to create reminder. Please try again.", ErrorType.Validation);

            return new ReminderServiceResult(true, "Reminder created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateReminder error for user {UserId}", userId);
            return new ReminderServiceResult(false, "An error occurred while creating the reminder", ErrorType.ServerError);
        }
    }

    public ReminderServiceResult EditReminder(EditReminderRequest request, int userId, int rid)
    {
        try
        {
            var success = reminderRepository.EditReminder(request, userId, rid);
            if (!success)
                return new ReminderServiceResult(false, "Reminder not found or update failed", ErrorType.NotFound);

            return new ReminderServiceResult(true, "Reminder updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EditReminder error for user {UserId}, reminder {ReminderId}", userId, rid);
            return new ReminderServiceResult(false, "An error occurred while updating the reminder", ErrorType.ServerError);
        }
    }

    public ReminderServiceResult DeleteReminder(int userId, int rid)
    {
        try
        {
            var success = reminderRepository.DeleteReminder(userId, rid);
            if (!success)
                return new ReminderServiceResult(false, "Reminder not found or already deleted", ErrorType.NotFound);

            return new ReminderServiceResult(true, "Reminder deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteReminder error for user {UserId}, reminder {ReminderId}", userId, rid);
            return new ReminderServiceResult(false, "An error occurred while deleting the reminder", ErrorType.ServerError);
        }
    }

    public ReminderServiceResult<List<ReminderResponse>> GetReminder(int userId)
    {
        try
        {
            var reminders = reminderRepository.GetReminder(userId);

            // Empty list is valid - user has no reminders yet (200 OK with empty array)
            var reminderList = reminders.Select(r => new ReminderResponse
            {
                Id = r.Id,
                BillName = r.BillName,
                DueDate = r.DueDate,
                PaymentMethod = r.PaymentMethod,
                Frequency = r.Frequency,
                NotificationTiming = r.NotificationTiming,
                Status = r.Status
            }).ToList();

            var message = reminderList.Any()
                ? "Reminders retrieved successfully"
                : "No reminders found";

            return new ReminderServiceResult<List<ReminderResponse>>(true, message, reminderList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetReminder error for user {UserId}", userId);
            return new ReminderServiceResult<List<ReminderResponse>>(false, "An error occurred while retrieving reminders", errorType: ErrorType.ServerError);
        }
    }

    public ReminderServiceResult<ReminderResponse> GetReminderById(int userId, int rid)
    {
        try
        {
            var result = reminderRepository.GetReminderById(userId, rid);
            if (!result.Any())
            {
                return new ReminderServiceResult<ReminderResponse>(false, "Reminder not found", errorType: ErrorType.NotFound);
            }

            var reminder = result.Select(r => new ReminderResponse
            {
                Id = r.Id,
                BillName = r.BillName,
                DueDate = r.DueDate,
                PaymentMethod = r.PaymentMethod,
                Frequency = r.Frequency,
                NotificationTiming = r.NotificationTiming,
                Status = r.Status
            }).FirstOrDefault();

            return new ReminderServiceResult<ReminderResponse>(true, "Reminder retrieved successfully", reminder!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetReminderById error for user {UserId}, reminder {ReminderId}", userId, rid);
            return new ReminderServiceResult<ReminderResponse>(false, "An error occurred while retrieving the reminder", errorType: ErrorType.ServerError);
        }
    }
}

public class ReminderServiceResult
{
    public bool Success { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    public ReminderServiceResult(bool success, string message, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        ErrorType = errorType;
    }
}

public class ReminderServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public ErrorType ErrorType { get; set; }

    public ReminderServiceResult(bool success, string message, T? data = default, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        Data = data;
        ErrorType = errorType;
    }
}