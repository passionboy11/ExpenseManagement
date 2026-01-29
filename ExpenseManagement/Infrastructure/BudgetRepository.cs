using Dapper;
using ExpenseManagement.DTO;


namespace ExpenseManagement.Infrastructure;

public interface IBudgetRepository
{
    bool CreateBudget(CreateBudget budget, int userId);
    bool EditBudget(EditBudget budget, int tid, int userId);
    bool DeleteBudget(DeleteBudget budget, int userId, int tid);
    IEnumerable<BudgetResponse> ReadBudget(int userId);
    IEnumerable<BudgetResponse> GetBudgetById(int userId, int Id);
    IEnumerable<BudgetResponse> ReadAllBudgets();
    decimal GetBudgetUsage(int userId, string category);
    decimal GetBudgetLimit(int userId, string category);

}

public class BudgetRepository:IBudgetRepository
{
    private readonly IDbConnectionFactory connectionFactory;
    public BudgetRepository(IDbConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
        
    }
    public bool CreateBudget(CreateBudget budget, int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        if (userId == 0)
            throw new InvalidOperationException("UserId must be set");

        var sql = @"INSERT INTO Budgets ( UserId, Category, LimitAmount, MonthYear) 
                        VALUES (@UserId, @Category, @LimitAmount, @MonthYear)";

        var result = connection.Execute(sql, new
        {
            UserId = userId,
            budget.Category,
            budget.LimitAmount,
            budget.MonthYear
        });

        return result > 0;
    }

    public bool EditBudget(EditBudget budget, int tid, int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"UPDATE Budgets SET Category = @Category, LimitAmount = @LimitAmount, MonthYear = @MonthYear WHERE UserId = @UserId AND Id = @tid";

        var result = connection.Execute(sql, new
        {
            budget.Id,
            UserId = userId,
            budget.Category,
            budget.LimitAmount,
            budget.MonthYear,
        });
        return result > 0;
    }

    public bool DeleteBudget(DeleteBudget budget, int userId, int tid)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"DELETE FROM Budgets WHERE  UserId = @UserId AND Id = @tid";
        var result = connection.Execute(sql, new
        {
            budget.Id,
            UserID = userId
        });
        return result > 0;
    }

    public IEnumerable<BudgetResponse> ReadBudget(int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"SELECT b.*, u.Email FROM Budgets b INNER JOIN UserAccounts u on b.UserId =u.Id WHERE b.UserId = @UserId";
        var result = connection.Query<BudgetResponse>(sql, new
        {
            UserId = userId
        });
        return result;
    }

    public IEnumerable<BudgetResponse> GetBudgetById(int userId, int Id)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Budgets WHERE UserId =@userId AND Id = @Id";
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
        INNER JOIN UserAccounts u ON b.UserId = u.Id;
    ";
        var result = connection.Query<BudgetResponse>(query);
        return result;
    }

    public decimal GetBudgetUsage(int userId, string category)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"SELECT IFNULL(SUM(Amount), 0)
                FROM Transactions
                WHERE UserId = @UserId AND IsExpense = 1 AND Category = @Category";
        return connection.QuerySingleOrDefault<decimal>(sql, new { UserId = userId, Category = category });
    }

    public decimal GetBudgetLimit(int userId, string category)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"SELECT LimitAmount
                FROM Budgets
                WHERE UserId = @UserId AND Category = @Category";
        return connection.QuerySingleOrDefault<decimal>(sql, new { UserId = userId, Category = category });
    }
}
