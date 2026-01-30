//using Dapper;
//using ExpenseManagement.DTO;


//namespace ExpenseManagement.Infrastructure;

//public interface IBudgetRepository
//{
//    bool CreateBudget(CreateBudget budget, int userId);
//    bool EditBudget(EditBudget budget, int tid, int userId);
//    bool DeleteBudget( int userId, int tid);
//    IEnumerable<BudgetResponse> ReadBudget(int userId);
//    IEnumerable<BudgetResponse> GetBudgetById(int userId, int Id);
//    IEnumerable<BudgetResponse> ReadAllBudgets();
//    decimal GetBudgetUsage(int userId, string category);
//    decimal GetBudgetLimit(int userId, string category);

//}

//public class BudgetRepository:IBudgetRepository
//{
//    private readonly IDbConnectionFactory connectionFactory;
//    public BudgetRepository(IDbConnectionFactory connectionFactory)
//    {
//        this.connectionFactory = connectionFactory;

//    }
//    public bool CreateBudget(CreateBudget budget, int userId)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        if (userId == 0)
//            throw new InvalidOperationException("UserId must be set");

//        var sql = @"INSERT INTO Budgets ( UserId, Category, LimitAmount, MonthYear) 
//                        VALUES (@UserId, @Category, @LimitAmount, @MonthYear)";

//        var result = connection.Execute(sql, new
//        {
//            UserId = userId,
//            budget.Category,
//            budget.LimitAmount,
//            budget.MonthYear
//        });

//        return result > 0;
//    }

//    public bool EditBudget(EditBudget budget, int tid, int userId)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"UPDATE Budgets SET Category = @Category, LimitAmount = @LimitAmount, MonthYear = @MonthYear WHERE UserId = @UserId AND Id = @tid";

//        var result = connection.Execute(sql, new
//        {
//            budget.Id,
//            UserId = userId,
//            budget.Category,
//            budget.LimitAmount,
//            budget.MonthYear,
//        });
//        return result > 0;
//    }

//    public bool DeleteBudget( int userId, int tid)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"DELETE FROM Budgets WHERE  UserId = @UserId AND Id = @tid";
//        var result = connection.Execute(sql, new
//        {
//           tid = tid,
//            UserId = userId
//        });
//        return result > 0;
//    }

//    public IEnumerable<BudgetResponse> ReadBudget(int userId)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"SELECT b.*, u.Email FROM Budgets b INNER JOIN UserAccounts u on b.UserId =u.Id WHERE b.UserId = @UserId";
//        var result = connection.Query<BudgetResponse>(sql, new
//        {
//            UserId = userId
//        });
//        return result;
//    }

//    public IEnumerable<BudgetResponse> GetBudgetById(int userId, int Id)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = "SELECT * FROM Budgets WHERE UserId =@userId AND Id = @Id";
//        var result = connection.Query<BudgetResponse>(sql, new
//        {
//            UserId = userId,
//            Id = Id
//        });
//        return result;
//    }

//    public IEnumerable<BudgetResponse> ReadAllBudgets()
//    {
//        using var connection = connectionFactory.CreateConnection();
//        string query = @"
//        SELECT 
//            b.Id,
//            b.UserId,
//            u.Email,
//            b.LimitAmount,
//            b.Category,
//            b.MonthYear,
//            b.CreatedAt
//        FROM Budgets b
//        INNER JOIN UserAccounts u ON b.UserId = u.Id;
//    ";
//        var result = connection.Query<BudgetResponse>(query);
//        return result;
//    }

//    public decimal GetBudgetUsage(int userId, string category)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"SELECT IFNULL(SUM(Amount), 0)
//                FROM Transactions
//                WHERE UserId = @UserId AND IsExpense = 1 AND Category = @Category";
//        return connection.QuerySingleOrDefault<decimal>(sql, new { UserId = userId, Category = category });
//    }

//    public decimal GetBudgetLimit(int userId, string category)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"SELECT LimitAmount
//                FROM Budgets
//                WHERE UserId = @UserId AND Category = @Category";
//        return connection.QuerySingleOrDefault<decimal>(sql, new { UserId = userId, Category = category });
//    }
//}
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

    // ✅ FIXED: Added validation and better error handling
    public bool CreateBudget(CreateBudget budget, int userId)
    {
        if (userId == 0)
            throw new InvalidOperationException("UserId must be set");

        using var connection = connectionFactory.CreateConnection();
        connection.Open();

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
            });

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
            Console.WriteLine($"Budget creation failed: {ex.Message}");
            throw;
        }
    }

    // ✅ FIXED: Proper validation and error handling
    public bool EditBudget(EditBudget budget, int tid, int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        try
        {
            // First check if budget exists
            var existsSql = @"
                SELECT COUNT(1)
                FROM Budgets
                WHERE Id = @Tid AND UserId = @UserId";

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
            });

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
            Console.WriteLine($"Budget edit failed: {ex.Message}");
            throw;
        }
    }

    // ✅ FIXED: Added validation and logging
    public bool DeleteBudget(int userId, int tid)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        try
        {
            // First check if budget exists
            var existsSql = @"
                SELECT Category, LimitAmount
                FROM Budgets
                WHERE Id = @Tid AND UserId = @UserId";

            var budget = connection.QuerySingleOrDefault<BudgetResponse>(existsSql, new
            {
                Tid = tid,
                UserId = userId
            });

            if (budget == null)
            {
                Console.WriteLine($"Budget not found: Id={tid}, UserId={userId}");
                return false;
            }

            var sql = @"
                DELETE FROM Budgets
                WHERE UserId = @UserId AND Id = @Tid";

            var result = connection.Execute(sql, new
            {
                Tid = tid,
                UserId = userId
            });

            if (result > 0)
            {
                Console.WriteLine($"Budget deleted: Id={tid}, Category={budget.Category}, UserId={userId}");
            }
            else
            {
                Console.WriteLine("Budget deletion failed: No rows affected");
            }

            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Budget deletion failed: {ex.Message}");
            throw;
        }
    }

    // ✅ FIXED: Added ORDER BY for consistent results
    public IEnumerable<BudgetResponse> ReadBudget(int userId)
    {
        using var connection = connectionFactory.CreateConnection();

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
            ORDER BY b.MonthYear DESC, b.Category ASC"; // ⚠️ Most recent first, alphabetical by category

        var result = connection.Query<BudgetResponse>(sql, new
        {
            UserId = userId
        });

        return result; // Returns empty collection if no budgets
    }

    // ✅ FIXED: Better query structure
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

        return result; // Returns empty collection if not found
    }

    // ✅ FIXED: Added ORDER BY
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
            ORDER BY b.MonthYear DESC, u.Email ASC, b.Category ASC"; // ⚠️ Ordered by month, user, category

        var result = connection.Query<BudgetResponse>(query);
        return result; // Returns empty collection if no budgets
    }

    // ✅ Already correct - using IFNULL for safety
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

        return usage; // Returns 0 if no expenses found (thanks to IFNULL)
    }

    // ✅ FIXED: Added COALESCE for null safety
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

        return limit; // Returns 0 if budget not found (thanks to COALESCE)
    }
}
