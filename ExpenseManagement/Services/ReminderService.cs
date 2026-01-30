//using ExpenseManagement.DTO.ReminderDTO;
//using ExpenseManagement.Infrastructure;

//namespace ExpenseManagement.Services;

//public interface IReminderService
//{
//    ReminderServiceResult CreateReminder (CreateReminderRequest request,int userId);
//    ReminderServiceResult EditReminder (EditReminderRequest request,int userId,int rid);
//    ReminderServiceResult DeleteReminder (int userId, int rid);

//    ReminderServiceResult<List<ReminderResponse>> GetReminder(int userId);
//    ReminderServiceResult <ReminderResponse> GetReminderById(int userId, int rid);
//}

//public class ReminderService:IReminderService
//{
//    private readonly IReminderRepository reminderRepository;
//    public ReminderService(IReminderRepository reminderRepository)
//    {
//        this.reminderRepository = reminderRepository;
//    }

//    public ReminderServiceResult CreateReminder(CreateReminderRequest request, int userId)
//    {
//        var success = reminderRepository.CreateReminder(request, userId);
//        if (!success)
//        {
//            return new ReminderServiceResult(false,"Reminder creation failed");
//        }
//        return new ReminderServiceResult(true, "Reminder successfully created");
//    }

//    public ReminderServiceResult EditReminder(EditReminderRequest request, int userId, int rid)
//    {
//        var success = reminderRepository.EditReminder(request, userId, rid);
//        if(!success)
//            return new ReminderServiceResult(false,"Reminder edit failed");

//        return new ReminderServiceResult(true, "Reminder successfully edited");
//    }

//    public ReminderServiceResult DeleteReminder(int userId, int rid)
//    {
//        var success = reminderRepository.DeleteReminder(userId, rid);
//        if (!success)
//        {
//            return new ReminderServiceResult(false,"Reminder deletion failed");
//        }
//        return new ReminderServiceResult(true, "Reminder successfully deleted");
//    }

//    public ReminderServiceResult<List<ReminderResponse>> GetReminder(int userId)
//    {
//        var success = reminderRepository.GetReminder(userId);
//        if (!success.Any())
//        {
//            return new ReminderServiceResult<List<ReminderResponse>>(false,"Reminder retrieval failed");
//        }
//        var reminders = success.Select(r => new ReminderResponse
//        {
//            Id = r.Id,
//            BillName = r.BillName,
//            DueDate = r.DueDate,
//            PaymentMethod = r.PaymentMethod,
//            Frequency = r.Frequency,
//            NotificationTiming = r.NotificationTiming,
//            Status = r.Status
//        }).ToList();
//        return new ReminderServiceResult<List<ReminderResponse>>(true, "Reminder successfully retrieved",reminders);
//    }
//    public ReminderServiceResult<ReminderResponse> GetReminderById(int userId,int rid)
//    {
//        var success = reminderRepository.GetReminderById(userId, rid);
//        if (!success.Any())
//        {
//            return new ReminderServiceResult<ReminderResponse>(false,"Reminder retrieval failed");
//        }
//        var reminder = success.Select(r => new ReminderResponse
//        {
//            Id = r.Id,
//            BillName = r.BillName,
//            DueDate = r.DueDate,
//            PaymentMethod = r.PaymentMethod,
//            Frequency = r.Frequency,
//            NotificationTiming = r.NotificationTiming,
//            Status = r.Status
//        }).FirstOrDefault();
//        return new ReminderServiceResult<ReminderResponse>(true, "Reminder successfully retrieved", reminder);
//    }
//}

//public class ReminderServiceResult
//{
//    public bool Success { get; }
//    public string Message { get; }
//    public ReminderServiceResult(bool success, string message)
//    {
//        Success = success;
//        Message = message;
//    }
//}

//public class ReminderServiceResult<T>
//{
//    public bool Success { get; set; }
//    public string Message { get; set; }
//    public T? Data { get; set; }

//    public ReminderServiceResult(bool success, string message, T? data=default)
//    {
//        Success = success;
//        Message = message;
//        Data = data;
//    }
//}
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

    public ReminderService(IReminderRepository reminderRepository)
    {
        this.reminderRepository = reminderRepository;
    }

    public ReminderServiceResult CreateReminder(CreateReminderRequest request, int userId)
    {
        try
        {
            var success = reminderRepository.CreateReminder(request, userId);
            if (!success)
                return new ReminderServiceResult(false, "Failed to create reminder. Please try again.");

            return new ReminderServiceResult(true, "Reminder created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateReminder error: {ex.Message}");
            return new ReminderServiceResult(false, "An error occurred while creating the reminder");
        }
    }

    public ReminderServiceResult EditReminder(EditReminderRequest request, int userId, int rid)
    {
        try
        {
            var success = reminderRepository.EditReminder(request, userId, rid);
            if (!success)
                return new ReminderServiceResult(false, "Reminder not found or update failed");

            return new ReminderServiceResult(true, "Reminder updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EditReminder error: {ex.Message}");
            return new ReminderServiceResult(false, "An error occurred while updating the reminder");
        }
    }

    public ReminderServiceResult DeleteReminder(int userId, int rid)
    {
        try
        {
            var success = reminderRepository.DeleteReminder(userId, rid);
            if (!success)
                return new ReminderServiceResult(false, "Reminder not found or already deleted");

            return new ReminderServiceResult(true, "Reminder deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteReminder error: {ex.Message}");
            return new ReminderServiceResult(false, "An error occurred while deleting the reminder");
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
            Console.WriteLine($"GetReminder error: {ex.Message}");
            return new ReminderServiceResult<List<ReminderResponse>>(false, "An error occurred while retrieving reminders");
        }
    }

    public ReminderServiceResult<ReminderResponse> GetReminderById(int userId, int rid)
    {
        try
        {
            var result = reminderRepository.GetReminderById(userId, rid);
            if (!result.Any())
            {
                return new ReminderServiceResult<ReminderResponse>(false, "Reminder not found");
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
            Console.WriteLine($"GetReminderById error: {ex.Message}");
            return new ReminderServiceResult<ReminderResponse>(false, "An error occurred while retrieving the reminder");
        }
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

    public ReminderServiceResult(bool success, string message, T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
