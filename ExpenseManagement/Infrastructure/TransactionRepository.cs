using Dapper;
using ExpenseManagement.DTO;

namespace ExpenseManagement.Infrastructure;

public interface ITransactionRepository
{
    bool CreateTransaction(CreateTransactionRequest request, int userId);
    bool EditTransaction(EditTransactionRequest request, int userId, int tid);
    bool DeleteTransaction(int userId, int tid);
    IEnumerable<TransactionResponse> ReadTransaction(int userId);
    IEnumerable<TransactionResponse> GetTransactionById(int userId, int Id);
    IEnumerable<TransactionResponse> ReadAllTransactions();
    decimal GetUserBalance(int id);
    bool UpdateUserBalance(int id, decimal amountChange);
}

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory;
    }

    public bool CreateTransaction(CreateTransactionRequest request, int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sql = @"
                INSERT INTO Transactions
                (UserId, Amount, IsExpense, Category, Description, PaymentMethod, IsRecurring, Date)
                VALUES
                (@UserId, @Amount, @IsExpense, @Category, @Description, @PaymentMethod, @IsRecurring, @Date)";

            var result = connection.Execute(sql, new
            {
                UserId = userId,
                request.Amount,
                request.IsExpense,
                request.Category,
                request.Description,
                request.PaymentMethod,
                request.IsRecurring,
                request.Date
            }, transaction);

            if (result == 0)
            {
                transaction.Rollback();
                Console.WriteLine("Insert failed: No rows affected");
                return false;
            }

            decimal balanceChange = request.IsExpense ? -request.Amount : request.Amount;

            var balanceSql = @"
                UPDATE UserAccounts
                SET Balance = Balance + @Change
                WHERE Id = @UserId";

            var balanceRows = connection.Execute(balanceSql, new
            {
                Change = balanceChange,
                UserId = userId
            }, transaction); 

            if (balanceRows == 0)
            {
                transaction.Rollback();
                Console.WriteLine("Balance update failed: User not found");
                return false;
            }

            transaction.Commit();
            Console.WriteLine($"Transaction created successfully. Amount: {request.Amount}, Balance change: {balanceChange}");
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.WriteLine($"Transaction creation failed: {ex.Message}");
            throw;
        }
    }

    public bool EditTransaction(EditTransactionRequest request, int userId, int tid)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();

        try
        {
           
            var old = connection.QuerySingleOrDefault<TransactionResponse>(
                @"SELECT Amount, IsExpense
                  FROM Transactions
                  WHERE Id = @Tid AND UserId = @UserId",
                new { Tid = tid, UserId = userId },
                tx
            );

            if (old == null)
            {
                tx.Rollback();
                Console.WriteLine($"Transaction not found: Id={tid}, UserId={userId}");
                return false;
            }

            var updateSql = @"
                UPDATE Transactions
                SET Amount = @Amount,
                    IsExpense = @IsExpense,
                    Category = @Category,
                    Description = @Description,
                    PaymentMethod = @PaymentMethod,
                    IsRecurring = @IsRecurring,
                    Date = @Date
                WHERE Id = @Tid AND UserId = @UserId";

            var rows = connection.Execute(updateSql, new
            {
                Tid = tid,
                UserId = userId,
                request.Amount,
                request.IsExpense,
                request.Category,
                request.Description,
                request.PaymentMethod,
                request.IsRecurring,
                request.Date 
            }, tx);

            if (rows == 0)
            {
                tx.Rollback();
                Console.WriteLine("Update failed: No rows affected");
                return false;
            }

            decimal oldChange = old.IsExpense ? -old.Amount : old.Amount;
            decimal newChange = request.IsExpense ? -request.Amount : request.Amount;
            decimal diff = newChange - oldChange;

            var balanceRows = connection.Execute(
                @"UPDATE UserAccounts
                  SET Balance = Balance + @Diff
                  WHERE Id = @UserId",
                new { Diff = diff, UserId = userId },
                tx
            );

            if (balanceRows == 0)
            {
                tx.Rollback();
                Console.WriteLine("Balance update failed: User not found");
                return false;
            }

            tx.Commit();
            Console.WriteLine($"Transaction edited. Old: {old.Amount}, New: {request.Amount}, Balance diff: {diff}");
            return true;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            Console.WriteLine($"Edit transaction failed: {ex.Message}");
            throw;
        }
    }

    public bool DeleteTransaction(int userId, int tid)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();

        try
        {
            var old = connection.QuerySingleOrDefault<TransactionResponse>(
                @"SELECT Amount, IsExpense
                  FROM Transactions
                  WHERE Id = @Tid AND UserId = @UserId",
                new { Tid = tid, UserId = userId },
                tx
            );

            if (old == null)
            {
                tx.Rollback();
                Console.WriteLine($"Transaction not found: Id={tid}, UserId={userId}");
                return false;
            }

            var deleteSql = @"
                DELETE FROM Transactions
                WHERE UserId = @UserId AND Id = @Tid";

            var result = connection.Execute(deleteSql, new
            {
                UserId = userId,
                Tid = tid
            }, tx);

            if (result == 0)
            {
                tx.Rollback();
                Console.WriteLine("Delete failed: No rows affected");
                return false;
            }

            decimal balanceReversal = old.IsExpense ? old.Amount : -old.Amount;

            var balanceSql = @"
                UPDATE UserAccounts
                SET Balance = Balance + @Change
                WHERE Id = @UserId";

            var balanceRows = connection.Execute(balanceSql, new
            {
                Change = balanceReversal,
                UserId = userId
            }, tx);

            if (balanceRows == 0)
            {
                tx.Rollback();
                Console.WriteLine("Balance reversal failed: User not found");
                return false;
            }

            tx.Commit();
            Console.WriteLine($"Transaction deleted. Id={tid}, Balance reversed: {balanceReversal}");
            return true;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            Console.WriteLine($"Delete transaction failed: {ex.Message}");
            throw;
        }
    }

    public IEnumerable<TransactionResponse> GetTransactionById(int userId, int Id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        var sql = @"
            SELECT * FROM Transactions
            WHERE UserId = @UserId AND Id = @Id";

        var result = connection.Query<TransactionResponse>(sql, new
        {
            UserId = userId,
            Id = Id
        });

        return result;
    }

    public IEnumerable<TransactionResponse> ReadTransaction(int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        var sql = @"
            SELECT * FROM Transactions
            WHERE UserId = @UserId
            ORDER BY Date DESC";

        var result = connection.Query<TransactionResponse>(sql, new
        {
            UserId = userId
        });

        return result; 
    }

    public IEnumerable<TransactionResponse> ReadAllTransactions()
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        var sql = @"
            SELECT
                t.Id,
                u.Email,
                t.Amount,
                t.IsExpense,
                t.Category,
                t.Description,
                t.PaymentMethod,
                t.IsRecurring,
                t.Date
            FROM Transactions t
            INNER JOIN UserAccounts u ON t.UserId = u.Id
            ORDER BY t.Date DESC";

        var result = connection.Query<TransactionResponse>(sql);
        return result;
    }

    public decimal GetUserBalance(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        var sql = @"
            SELECT COALESCE(Balance, 0)
            FROM UserAccounts
            WHERE Id = @Id";

        return connection.QuerySingleOrDefault<decimal>(sql, new { Id = id });
        
    }

    public bool UpdateUserBalance(int id, decimal amountChange)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        var sql = @"
            UPDATE UserAccounts
            SET Balance = Balance + @AmountChange
            WHERE Id = @Id";

        var result = connection.Execute(sql, new
        {
            Id = id,
            AmountChange = amountChange
        });

        Console.WriteLine($"Balance updated: UserId={id}, Change={amountChange}, Rows affected={result}");
        return result > 0;
    }
}
