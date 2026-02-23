using Dapper;
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

    public bool CreateReminder(CreateReminderRequest request, int userId)
    {
        if (userId == 0)
            throw new InvalidOperationException("UserId must be set");

        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var txn = connection.BeginTransaction();

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
            },transaction:txn);

            txn.Commit();

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
            txn.Rollback();
            Console.WriteLine($"Reminder creation failed: {ex.Message}");
            throw;
        }
    }

    public bool EditReminder(EditReminderRequest request, int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var txn = connection.BeginTransaction();

        try
        {
            var existsSql = @"
                SELECT 1
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
                Frequency = request.Frequency.ToString(),
                Status = request.Status.ToString(), 
                NotificationTiming = request.NotificationTiming,
                UserId = userId,
                Rid = rid 
            },transaction:txn);
            txn.Commit();

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
            txn.Rollback();
            Console.WriteLine($"Reminder edit failed: {ex.Message}");
            throw;
        }
    }

    public bool DeleteReminder(int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
        using var txn = connection.BeginTransaction();

        try
        {

            var sql = @"
                DELETE FROM Reminder
                WHERE UserAccountId = @UserId AND Id = @Rid";

            var result = connection.Execute(sql, new
            {
                UserId = userId, 
                Rid = rid 
            },transaction:txn);

            txn.Commit();

            if (result > 0)
            {
                Console.WriteLine($"Reminder deleted: Id={rid}, UserId={userId}");
            }
            else
            {
                Console.WriteLine("Reminder deletion failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            txn.Rollback();
            Console.WriteLine($"Reminder deletion failed: {ex.Message}");
            throw;
        }
    }
    public IEnumerable<ReminderResponse> ReadAllReminders()
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();
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
            ORDER BY b.DueDate ASC, u.Email ASC"; 

        var result = connection.Query<ReminderResponse>(query);
        return result; 
    }

    public IEnumerable<ReminderResponse> GetReminder(int userId)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();

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
            ORDER BY DueDate ASC, BillName ASC"; 

        var result = connection.Query<ReminderResponse>(sql, new
        {
            UserId = userId
        });

        return result; 
    }
    public IEnumerable<ReminderResponse> GetReminderById(int userId, int rid)
    {
        using var connection = dbConnectionFactory.CreateConnection();
        connection.Open();

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
            UserId = userId, 
            Rid = rid 
        });

        return result; 
    }
}

