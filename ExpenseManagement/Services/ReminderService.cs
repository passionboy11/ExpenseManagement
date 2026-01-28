using ExpenseManagement.DTO.ReminderDTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface IReminderService
{
    ReminderServiceResult CreateReminder (CreateReminderRequest request,int userId);
    ReminderServiceResult EditReminder (EditReminderRequest request,int userId,int rid);
    ReminderServiceResult DeleteReminder (int userId, int rid);

    ReminderServiceResult<List<ReminderResponse>> GetReminder(int userId);
    ReminderServiceResult <ReminderResponse> GetReminderById(int userId, int rid);
}

public class ReminderService:IReminderService
{
    private readonly DataAccess dataAccess;

    public ReminderService(DataAccess dataAccess)
    {
        this.dataAccess = dataAccess;
    }

    public ReminderServiceResult CreateReminder(CreateReminderRequest request, int userId)
    {
        var success = dataAccess.CreateReminder(request, userId);
        if (!success)
        {
            return new ReminderServiceResult(false,"Reminder creation failed");
        }
        return new ReminderServiceResult(true, "Reminder successfully created");
    }

    public ReminderServiceResult EditReminder(EditReminderRequest request, int userId, int rid)
    {
        var success = dataAccess.EditReminder(request, userId, rid);
        if(!success)
            return new ReminderServiceResult(false,"Reminder edit failed");
        
        return new ReminderServiceResult(true, "Reminder successfully edited");
    }

    public ReminderServiceResult DeleteReminder(int userId, int rid)
    {
        var success = dataAccess.DeleteReminder(userId, rid);
        if (!success)
        {
            return new ReminderServiceResult(false,"Reminder deletion failed");
        }
        return new ReminderServiceResult(true, "Reminder successfully deleted");
    }

    public ReminderServiceResult<List<ReminderResponse>> GetReminder(int userId)
    {
        var success = dataAccess.GetReminder(userId);
        if (!success.Any())
        {
            return new ReminderServiceResult<List<ReminderResponse>>(false,"Reminder retrieval failed");
        }
        var reminders = success.Select(r => new ReminderResponse
        {
            BillName = r.BillName,
            DueDate = r.DueDate,
            PaymentMethod = r.PaymentMethod,
            Frequency = r.Frequency,
            NotificationTiming = r.NotificationTiming,
            Status = r.Status
        }).ToList();
        return new ReminderServiceResult<List<ReminderResponse>>(true, "Reminder successfully retrieved",reminders);
    }
    public ReminderServiceResult<ReminderResponse> GetReminderById(int userId,int rid)
    {
        var success = dataAccess.GetReminderById(userId, rid);
        if (!success.Any())
        {
            return new ReminderServiceResult<ReminderResponse>(false,"Reminder retrieval failed");
        }
        var reminder = success.Select(r => new ReminderResponse
        {
            BillName = r.BillName,
            DueDate = r.DueDate,
            PaymentMethod = r.PaymentMethod,
            Frequency = r.Frequency,
            NotificationTiming = r.NotificationTiming,
            Status = r.Status
        }).FirstOrDefault();
        return new ReminderServiceResult<ReminderResponse>(true, "Reminder successfully retrieved", reminder);
    }
}

public class ReminderServiceResult
{
    public bool Success { get; }
    public string Message { get; }
    public ReminderServiceResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

public class ReminderServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public ReminderServiceResult(bool success, string message, T? data=default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}