using Dapper;
using ExpenseManagement.DTO.ReminderDTO;

namespace ExpenseManagement.Infrastructure;
public interface IReminderRepository
    {
        bool CreateReminder(CreateReminderRequest request, int userId);
        bool EditReminder(EditReminderRequest request, int userId, int rid);
        bool DeleteReminder(int userId,int rid);
        IEnumerable<ReminderResponse> GetReminder(int userId);
        IEnumerable<ReminderResponse> GetReminderById(int userId, int rid);

    }
public class ReminderRepository:IReminderRepository
    {
    private readonly IDbConnectionFactory dbConnectionFactory;
    public ReminderRepository(IDbConnectionFactory dbConnectionFactory)
    {
        this.dbConnectionFactory = dbConnectionFactory;
    }
    public bool CreateReminder(CreateReminderRequest request, int userId)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        var sql =
            "INSERT INTO Reminder (UserAccountId,BillName,DueDate,PaymentMethod,Frequency,NotificationTiming) VALUES (@UserAccountId,@BillName,@DueDate,@PaymentMethod,@Frequency,@NotificationTiming)";
        var result = connection.Execute(sql, new
        {
            UserAccountId = userId,
            BillName = request.BillName,
            DueDate = request.DueDate,
            PaymentMethod = request.PaymentMethod,
            Frequency = request.Frequency,
            NotificationTiming = request.NotificationTiming
        });
        return result > 0;
    }

    public bool EditReminder(EditReminderRequest request, int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        var sql = "UPDATE Reminder SET BillName=@BillName, DueDate=@DueDate, PaymentMethod=@PaymentMethod, Frequency=@Frequency, NotificationTiming=@NotificationTiming WHERE UserAccountId = @userId AND Id = @rid";
        var result = connection.Execute(sql, new
        {
            BillName = request.BillName,
            DueDate = request.DueDate,
            PaymentMethod = request.PaymentMethod,
            Frequency = request.Frequency.ToString(), // if DB is ENUM('Daily','Weekly',..)
            NotificationTiming = request.NotificationTiming,
            userId = userId,
            rid = rid
        });
        return result > 0;
    }

    public bool DeleteReminder(int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        var sql = "DELETE FROM Reminder WHERE UserAccountId = @userId AND Id = @rid";
        var result = connection.Execute(sql, new { userId = userId, rid = rid });
        return result > 0;
    }
    public IEnumerable<ReminderResponse> GetReminder(int userId)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        var sql = @"SELECT * FROM Reminder WHERE UserAccountId = @UserId";
        var result = connection.Query<ReminderResponse>(sql, new
        {
            UserId = userId,
        });
        return result;
    }
    public IEnumerable<ReminderResponse> GetReminderById(int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        var sql = "SELECT * FROM Reminder WHERE UserAccountId = @userId AND Id = @rid";
        var result = connection.Query<ReminderResponse>(sql, new
        {
            userId = userId,
            rid = rid
        });
        return result;
    }
}

