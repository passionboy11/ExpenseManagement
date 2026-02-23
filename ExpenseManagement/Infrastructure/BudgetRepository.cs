using Dapper;
using ExpenseManagement.DTO;

namespace ExpenseManagement.Infrastructure;

public interface IBudgetRepository
{
    bool CreateBudget(CreateBudget budget, int userId);
    bool EditBudget(EditBudget budget, int tid, int userId);
    bool DeleteBudget(int userId, int tid);
    IEnumerable<BudgetResponse> ReadBudget(int userId);
    IEnumerable<BudgetResponse> GetBudgetById(int userId, int Id);
    IEnumerable<BudgetResponse> ReadAllBudgets();
    decimal GetBudgetUsage(int userId, string category);
    decimal GetBudgetLimit(int userId, string category);
}

public class BudgetRepository : IBudgetRepository
{
    private readonly IDbConnectionFactory connectionFactory;

    public BudgetRepository(IDbConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public bool CreateBudget(CreateBudget budget, int userId)
    {
        if (userId == 0)
            throw new InvalidOperationException("UserId must be set");

        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var txn = connection.BeginTransaction();

        try
        {
            var sql = @"
                INSERT INTO Budgets (UserId, Category, LimitAmount, MonthYear)
                VALUES (@UserId, @Category, @LimitAmount, @MonthYear)";

            var result = connection.Execute(sql, new
            {
                UserId = userId,
                budget.Category,
                budget.LimitAmount,
                budget.MonthYear
            },transaction: txn);

            txn.Commit();

            if (result > 0)
            {
                Console.WriteLine($"Budget created: Category={budget.Category}, Limit={budget.LimitAmount}, UserId={userId}");
            }
            else
            {
                Console.WriteLine("Budget creation failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            txn.Rollback();
            Console.WriteLine($"Budget creation failed: {ex.Message}");
            throw;
        }
    }

   
    public bool EditBudget(EditBudget budget, int tid, int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var txn = connection.BeginTransaction();

        try
        {
            var existsSql = @"
                SELECT 1 FROM Budgets WHERE Id = @Tid AND UserId = @UserId";

            var exists = connection.QuerySingleOrDefault<int>(existsSql, new
            {
                Tid = tid,
                UserId = userId
            });

            if (exists == 0)
            {
                Console.WriteLine($"Budget not found: Id={tid}, UserId={userId}");
                return false;
            }

            var sql = @"
                UPDATE Budgets
                SET Category = @Category,
                    LimitAmount = @LimitAmount,
                    MonthYear = @MonthYear
                WHERE UserId = @UserId AND Id = @Tid";

            var result = connection.Execute(sql, new
            {
                Tid = tid,
                UserId = userId,
                budget.Category,
                budget.LimitAmount,
                budget.MonthYear
            },transaction:txn);

            txn.Commit();

            if (result > 0)
            {
                Console.WriteLine($"Budget updated: Id={tid}, Category={budget.Category}, Limit={budget.LimitAmount}");
            }
            else
            {
                Console.WriteLine("Budget update failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            txn.Rollback();
            Console.WriteLine($"Budget edit failed: {ex.Message}");
            throw;
        }
    }

    
    public bool DeleteBudget(int userId, int tid)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var txn = connection.BeginTransaction();

        try
        {
            var sql = @"
                DELETE FROM Budgets
                WHERE UserId = @UserId AND Id = @Tid";

            var result = connection.Execute(sql, new
            {
                Tid = tid,
                UserId = userId
            },transaction: txn);

            txn.Commit();

            if (result > 0)
            {
                Console.WriteLine($"Budget deleted: Id={tid}, UserId={userId}");
            }
            else
            {
                Console.WriteLine("Budget deletion failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            txn.Rollback();
            Console.WriteLine($"Budget deletion failed: {ex.Message}");
            throw;
        }
    }


    public IEnumerable<BudgetResponse> ReadBudget(int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        var sql = @"
            SELECT
                b.Id,
                b.UserId,
                u.Email,
                b.Category,
                b.LimitAmount,
                b.MonthYear,
                b.CreatedAt
            FROM Budgets b
            INNER JOIN UserAccounts u ON b.UserId = u.Id
            WHERE b.UserId = @UserId
            ORDER BY b.MonthYear DESC, b.Category ASC";

        var result = connection.Query<BudgetResponse>(sql, new
        {
            UserId = userId
        });

        return result; 
    }

    
    public IEnumerable<BudgetResponse> GetBudgetById(int userId, int Id)
    {
        using var connection = connectionFactory.CreateConnection();

        var sql = @"
            SELECT
                b.Id,
                b.UserId,
                b.Category,
                b.LimitAmount,
                b.MonthYear,
                b.CreatedAt
            FROM Budgets b
            WHERE b.UserId = @UserId AND b.Id = @Id";

        var result = connection.Query<BudgetResponse>(sql, new
        {
            UserId = userId,
            Id = Id
        });

        return result; 
    }

    public IEnumerable<BudgetResponse> ReadAllBudgets()
    {
        using var connection = connectionFactory.CreateConnection();

        string query = @"
            SELECT
                b.Id,
                b.UserId,
                u.Email,
                b.LimitAmount,
                b.Category,
                b.MonthYear,
                b.CreatedAt
            FROM Budgets b
            INNER JOIN UserAccounts u ON b.UserId = u.Id
            ORDER BY b.MonthYear DESC, u.Email ASC, b.Category ASC";

        var result = connection.Query<BudgetResponse>(query);
        return result;
    }

  
    public decimal GetBudgetUsage(int userId, string category)
    {
        using var connection = connectionFactory.CreateConnection();

        var sql = @"
            SELECT IFNULL(SUM(Amount), 0)
            FROM Transactions
            WHERE UserId = @UserId
              AND IsExpense = 1
              AND Category = @Category";

        var usage = connection.QuerySingleOrDefault<decimal>(sql, new
        {
            UserId = userId,
            Category = category
        });

        return usage; 
    }

    public decimal GetBudgetLimit(int userId, string category)
    {
        using var connection = connectionFactory.CreateConnection();

        var sql = @"
            SELECT COALESCE(LimitAmount, 0)
            FROM Budgets
            WHERE UserId = @UserId
              AND Category = @Category
            LIMIT 1";

        var limit = connection.QuerySingleOrDefault<decimal>(sql, new
        {
            UserId = userId,
            Category = category
        });

        return limit; 
    }
}
