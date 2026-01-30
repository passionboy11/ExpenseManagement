//using Dapper;
//using ExpenseManagement.DTO;
//using ExpenseManagement.DTO.ReminderDTO;
//using Microsoft.AspNetCore.Connections;

//namespace ExpenseManagement.Infrastructure;
//public interface IReminderRepository
//    {
//        bool CreateReminder(CreateReminderRequest request, int userId);
//        bool EditReminder(EditReminderRequest request, int userId, int rid);
//        bool DeleteReminder(int userId,int rid);
//        IEnumerable<ReminderResponse> ReadAllReminders();
//        IEnumerable<ReminderResponse> GetReminder(int userId);
//        IEnumerable<ReminderResponse> GetReminderById(int userId, int rid);

//    }
//public class ReminderRepository:IReminderRepository
//    {
//    private readonly IDbConnectionFactory dbConnectionFactory;
//    public ReminderRepository(IDbConnectionFactory dbConnectionFactory)
//    {
//        this.dbConnectionFactory = dbConnectionFactory;
//    }
//    public bool CreateReminder(CreateReminderRequest request, int userId)
//    {
//        using var connection = dbConnectionFactory.CreateConnection();
//        var sql =
//            "INSERT INTO Reminder (UserAccountId,BillName,DueDate,PaymentMethod,Frequency,NotificationTiming) VALUES (@UserAccountId,@BillName,@DueDate,@PaymentMethod,@Frequency,@NotificationTiming)";
//        var result = connection.Execute(sql, new
//        {
//            UserAccountId = userId,
//            BillName = request.BillName,
//            DueDate = request.DueDate,
//            PaymentMethod = request.PaymentMethod,
//            Frequency = request.Frequency,
//            NotificationTiming = request.NotificationTiming
//        });
//        return result > 0;
//    }

//    public bool EditReminder(EditReminderRequest request, int userId, int rid)
//    {
//        using var connection = dbConnectionFactory.CreateConnection();
//        var sql = "UPDATE Reminder SET BillName=@BillName, DueDate=@DueDate, PaymentMethod=@PaymentMethod, Frequency=@Frequency,Status=@Status, NotificationTiming=@NotificationTiming WHERE UserAccountId = @userId AND Id = @rid";
//        var result = connection.Execute(sql, new
//        {
//            BillName = request.BillName,
//            DueDate = request.DueDate,
//            PaymentMethod = request.PaymentMethod,
//            Frequency = request.Frequency.ToString(),
//            Status = request.Status.ToString(),/// if DB is ENUM('Daily','Weekly',..)
//            NotificationTiming = request.NotificationTiming,
//            userId = userId,
//            rid = rid
//        });
//        return result > 0;
//    }

//    public bool DeleteReminder(int userId, int rid)
//    {
//        using var connection = dbConnectionFactory.CreateConnection();
//        var sql = "DELETE FROM Reminder WHERE UserAccountId = @userId AND Id = @rid";
//        var result = connection.Execute(sql, new { userId = userId, rid = rid });
//        return result > 0;
//    }
//    public IEnumerable<ReminderResponse> ReadAllReminders()
//    {
//        using var connection = dbConnectionFactory.CreateConnection();
//        string query = @"
//        SELECT 
//            b.Id,
//            u.Id,
//            u.Email,
//            b.BillName,
//            b.DueDate,
//            b.PaymentMethod,
//            b.Frequency,
//            b.NotificationTiming,
//            b.Status
//        FROM Reminder b
//        INNER JOIN UserAccounts u ON b.UserAccountId  = u.Id;
//    ";
//        var result = connection.Query<ReminderResponse>(query);
//        return result;
//    }
//    public IEnumerable<ReminderResponse> GetReminder(int userId)
//    {
//        using var connection = dbConnectionFactory.CreateConnection();
//        var sql = @"SELECT * FROM Reminder WHERE UserAccountId = @UserId";
//        var result = connection.Query<ReminderResponse>(sql, new
//        {
//            UserId = userId,
//        });
//        return result;
//    }
//    public IEnumerable<ReminderResponse> GetReminderById(int userId, int rid)
//    {
//        using var connection = dbConnectionFactory.CreateConnection();
//        var sql = "SELECT * FROM Reminder WHERE UserAccountId = @userId AND Id = @rid";
//        var result = connection.Query<ReminderResponse>(sql, new
//        {
//            userId = userId,
//            rid = rid
//        });
//        return result;
//    }
//}
using Dapper;
using ExpenseManagement.DTO;
using ExpenseManagement.DTO.ReminderDTO;

namespace ExpenseManagement.Infrastructure;

public interface IReminderRepository
{
    bool CreateReminder(CreateReminderRequest request, int userId);
    bool EditReminder(EditReminderRequest request, int userId, int rid);
    bool DeleteReminder(int userId, int rid);
    IEnumerable<ReminderResponse> ReadAllReminders();
    IEnumerable<ReminderResponse> GetReminder(int userId);
    IEnumerable<ReminderResponse> GetReminderById(int userId, int rid);
}

public class ReminderRepository : IReminderRepository
{
    private readonly IDbConnectionFactory dbConnectionFactory;

    public ReminderRepository(IDbConnectionFactory dbConnectionFactory)
    {
        this.dbConnectionFactory = dbConnectionFactory;
    }

    // ✅ FIXED: Added validation, error handling, and logging
    public bool CreateReminder(CreateReminderRequest request, int userId)
    {
        if (userId == 0)
            throw new InvalidOperationException("UserId must be set");

        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();

        try
        {
            var sql = @"
                INSERT INTO Reminder
                (UserAccountId, BillName, DueDate, PaymentMethod, Frequency, NotificationTiming)
                VALUES
                (@UserAccountId, @BillName, @DueDate, @PaymentMethod, @Frequency, @NotificationTiming)";

            var result = connection.Execute(sql, new
            {
                UserAccountId = userId,
                BillName = request.BillName,
                DueDate = request.DueDate,
                PaymentMethod = request.PaymentMethod,
                Frequency = request.Frequency,
                NotificationTiming = request.NotificationTiming
            });

            if (result > 0)
            {
                Console.WriteLine($"Reminder created: BillName={request.BillName}, DueDate={request.DueDate}, UserId={userId}");
            }
            else
            {
                Console.WriteLine("Reminder creation failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reminder creation failed: {ex.Message}");
            throw;
        }
    }

    // ✅ FIXED: Parameter naming consistency, validation, error handling
    public bool EditReminder(EditReminderRequest request, int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();

        try
        {
            // First check if reminder exists
            var existsSql = @"
                SELECT COUNT(1)
                FROM Reminder
                WHERE Id = @Rid AND UserAccountId = @UserId";

            var exists = connection.QuerySingleOrDefault<int>(existsSql, new
            {
                Rid = rid,
                UserId = userId
            });

            if (exists == 0)
            {
                Console.WriteLine($"Reminder not found: Id={rid}, UserId={userId}");
                return false;
            }

            var sql = @"
                UPDATE Reminder
                SET BillName = @BillName,
                    DueDate = @DueDate,
                    PaymentMethod = @PaymentMethod,
                    Frequency = @Frequency,
                    Status = @Status,
                    NotificationTiming = @NotificationTiming
                WHERE UserAccountId = @UserId AND Id = @Rid";

            var result = connection.Execute(sql, new
            {
                BillName = request.BillName,
                DueDate = request.DueDate,
                PaymentMethod = request.PaymentMethod,
                Frequency = request.Frequency.ToString(), // Convert enum to string
                Status = request.Status.ToString(), // Convert enum to string
                NotificationTiming = request.NotificationTiming,
                UserId = userId, // ⚠️ FIX: Consistent naming (was lowercase userId)
                Rid = rid // ⚠️ FIX: Consistent naming (was lowercase rid)
            });

            if (result > 0)
            {
                Console.WriteLine($"Reminder updated: Id={rid}, BillName={request.BillName}, Status={request.Status}");
            }
            else
            {
                Console.WriteLine("Reminder update failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reminder edit failed: {ex.Message}");
            throw;
        }
    }

    // ✅ FIXED: Parameter naming consistency, validation, logging
    public bool DeleteReminder(int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();

        try
        {
            // First check if reminder exists and get details for logging
            var existsSql = @"
                SELECT BillName, DueDate
                FROM Reminder
                WHERE Id = @Rid AND UserAccountId = @UserId";

            var reminder = connection.QuerySingleOrDefault<ReminderResponse>(existsSql, new
            {
                Rid = rid,
                UserId = userId
            });

            if (reminder == null)
            {
                Console.WriteLine($"Reminder not found: Id={rid}, UserId={userId}");
                return false;
            }

            var sql = @"
                DELETE FROM Reminder
                WHERE UserAccountId = @UserId AND Id = @Rid";

            var result = connection.Execute(sql, new
            {
                UserId = userId, // ⚠️ FIX: Consistent naming
                Rid = rid // ⚠️ FIX: Consistent naming
            });

            if (result > 0)
            {
                Console.WriteLine($"Reminder deleted: Id={rid}, BillName={reminder.BillName}, UserId={userId}");
            }
            else
            {
                Console.WriteLine("Reminder deletion failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reminder deletion failed: {ex.Message}");
            throw;
        }
    }

    // ✅ FIXED: Explicit columns, proper column aliases, ORDER BY
    public IEnumerable<ReminderResponse> ReadAllReminders()
    {
        using var connection = dbConnectionFactory.CreateConnection();

        string query = @"
            SELECT
                b.Id,
                b.UserAccountId,
                u.Email,
                b.BillName,
                b.DueDate,
                b.PaymentMethod,
                b.Frequency,
                b.NotificationTiming,
                b.Status,
                b.CreatedAt
            FROM Reminder b
            INNER JOIN UserAccounts u ON b.UserAccountId = u.Id
            ORDER BY b.DueDate ASC, u.Email ASC"; // ⚠️ Sort by due date (upcoming first)

        var result = connection.Query<ReminderResponse>(query);
        return result; // Returns empty collection if no reminders
    }

    // ✅ FIXED: Explicit columns, ORDER BY, consistent parameter naming
    public IEnumerable<ReminderResponse> GetReminder(int userId)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        var sql = @"
            SELECT
                Id,
                UserAccountId,
                BillName,
                DueDate,
                PaymentMethod,
                Frequency,
                NotificationTiming,
                Status,
                CreatedAt
            FROM Reminder
            WHERE UserAccountId = @UserId
            ORDER BY DueDate ASC, BillName ASC"; // ⚠️ Upcoming bills first

        var result = connection.Query<ReminderResponse>(sql, new
        {
            UserId = userId
        });

        return result; // Returns empty collection if no reminders
    }

    // ✅ FIXED: Explicit columns, consistent parameter naming
    public IEnumerable<ReminderResponse> GetReminderById(int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();

        var sql = @"
            SELECT
                Id,
                UserAccountId,
                BillName,
                DueDate,
                PaymentMethod,
                Frequency,
                NotificationTiming,
                Status,
                CreatedAt
            FROM Reminder
            WHERE UserAccountId = @UserId AND Id = @Rid";

        var result = connection.Query<ReminderResponse>(sql, new
        {
            UserId = userId, // ⚠️ FIX: Consistent naming
            Rid = rid // ⚠️ FIX: Consistent naming
        });

        return result; // Returns empty collection if not found
    }
}

