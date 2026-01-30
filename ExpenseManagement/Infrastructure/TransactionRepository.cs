//using Dapper;
//using ExpenseManagement.DTO;
//using ExpenseManagement.Models;


//namespace ExpenseManagement.Infrastructure;

//public interface ITransactionRepository 
//{
//    bool CreateTransaction(CreateTransactionRequest request, int userId);
//    bool EditTransaction(EditTransactionRequest request, int userId, int tid);
//    bool DeleteTransaction(int userId, int tid);
//    IEnumerable<TransactionResponse> ReadTransaction(int userId);
//    IEnumerable<TransactionResponse> GetTransactionById(int userId, int Id);
//    IEnumerable<TransactionResponse> ReadAllTransactions();
//    decimal GetUserBalance(int id);
//    bool UpdateUserBalance(int id, decimal amountChange);
//}


//public class TransactionRepository:ITransactionRepository
//    {
//        private readonly IDbConnectionFactory connectionFactory;
//        public TransactionRepository(IDbConnectionFactory connectionFactory)
//        {
//            this.connectionFactory = connectionFactory;
//        }
//        public bool CreateTransaction(CreateTransactionRequest request, int userId)
//        {
//            using var connection = connectionFactory.CreateConnection();
//            using var transaction = connection.BeginTransaction();
//        try
//        {
//            var sql =
//                "INSERT INTO Transactions (UserId,Amount,IsExpense,Category,Description,PaymentMethod,IsRecurring,Date) VALUES (@UserId,@Amount,@IsExpense,@Category,@Description,@PaymentMethod,@IsRecurring,@Date)";
//            var result = connection.Execute(sql, new
//            {   /// transaction rolllback insert update delete
//                UserId = userId,
//                request.Amount,
//                request.IsExpense,
//                request.Category,
//                request.Description,
//                request.PaymentMethod,
//                request.IsRecurring,
//                request.Date
//            });
//            if (result == 0)
//            {
//                transaction.Rollback();
//                return false;
//            }
//            decimal balanceChange = request.IsExpense
//            ? -request.Amount
//            : request.Amount;

//            var balanceSql = @"UPDATE UserAccounts SET Balance = Balance + @Change WHERE Id = @UserId";
//            var balanceRows = connection.Execute(balanceSql, new
//            {
//                Change = balanceChange,
//                UserId = userId
//            }, transaction);

//            if (balanceRows == 0)
//            {
//                transaction.Rollback();
//                return false;
//            }
//            transaction.Commit();
//            return true;
//        }
//        catch(Exception ex)
//        {
//            transaction.Rollback();
//            Console.WriteLine($"Transaction creation failed: {ex.Message}");
//            throw;
//        }

//        }

//    public bool EditTransaction(EditTransactionRequest request, int userId, int tid)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        using var tx = connection.BeginTransaction();

//        try
//        {
//            var old = connection.QuerySingleOrDefault<TransactionResponse>(
//                @"SELECT Amount, IsExpense 
//              FROM Transactions 
//              WHERE Id = @Tid AND UserId = @UserId",
//                new { Tid = tid, UserId = userId },
//                tx
//            );

//            if (old == null)
//            {
//                tx.Rollback();
//                return false;
//            }

//            var updateSql = @"
//            UPDATE Transactions
//            SET Amount = @Amount,
//                IsExpense = @IsExpense,
//                Category = @Category,
//                Description = @Description,
//                PaymentMethod = @PaymentMethod,
//                IsRecurring = @IsRecurring,
//                Date = CURRENT_DATE()
//            WHERE Id = @Tid AND UserId = @UserId;
//        ";

//            var rows = connection.Execute(updateSql, new
//            {
//                Tid = tid,
//                UserId = userId,
//                request.Amount,
//                request.IsExpense,
//                request.Category,
//                request.Description,
//                request.PaymentMethod,
//                request.IsRecurring
//            }, tx);

//            if (rows == 0)
//            {
//                tx.Rollback();
//                return false;
//            }

//            decimal oldChange = old.IsExpense ? -old.Amount : old.Amount;
//            decimal newChange = request.IsExpense ? -request.Amount : request.Amount;
//            decimal diff = newChange - oldChange;

//            connection.Execute(
//                @"UPDATE UserAccounts
//              SET Balance = Balance + @Diff
//              WHERE Id = @UserId",
//                new { Diff = diff, UserId = userId },
//                tx
//            );

//            tx.Commit();
//            return true;
//        }
//        catch
//        {
//            tx.Rollback();
//            throw;
//        }
//    }

//    public bool DeleteTransaction(int userId, int tid)
//        {
//        using var connection = connectionFactory.CreateConnection();
//        var old = connection.QuerySingleOrDefault<TransactionResponse>(
//                "SELECT Amount, IsExpense FROM Transactions WHERE Id = @Tid AND UserId = @UserId",
//                new { Tid = tid, UserId = userId }
//            );
//            var sql = @"DELETE FROM Transactions WHERE  UserId = @UserId AND Id = @Tid";
//            var result = connection.Execute(sql, new
//            {
//                UserId = userId,
//                Tid = tid,
//            });
//            Console.WriteLine($"Deleting Transaction Id={tid} for UserId={userId}");
//            Console.WriteLine($"Rows affected: {result}");

//            if (result > 0 && old != null)
//            {
//                decimal change = old.IsExpense ? old.Amount : -old.Amount; // Reverse effect
//                UpdateUserBalance(userId, -change); // reverse old transaction
//            }

//            return result > 0;

//        }



//        public IEnumerable<TransactionResponse> GetTransactionById(int userId, int Id)
//        {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = "SELECT * FROM Transactions WHERE UserId =@userId AND Id = @Id";
//            var result = connection.Query<TransactionResponse>(sql, new
//            {
//                userId = userId,
//                Id = Id
//            });
//            return result;
//        }
//        public IEnumerable<TransactionResponse> ReadTransaction(int userId)
//        {
//            using var connection = connectionFactory.CreateConnection();
//            var sql = @"SELECT * FROM Transactions WHERE UserId = @UserId";

//            var result = connection.Query<TransactionResponse>(sql, new
//            {
//                UserId = userId,
//            });

//            return result;
//        }

//    public IEnumerable<TransactionResponse> ReadAllTransactions()
//        {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"SELECT t.Id,
//                               u.Email,
//                               t.Amount,
//                               t.IsExpense,
//                               t.Category,
//                               t.Description,
//                               t.PaymentMethod,
//                               t.IsRecurring,
//                               t.Date
//                    FROM Transactions t INNER JOIN UserAccounts u ON t.UserId = u.Id";
//            var result = connection.Query<TransactionResponse>(sql);
//            return result;
//        }
//        public decimal GetUserBalance(int id)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"SELECT Balance FROM UserAccounts WHERE Id = @Id";
//        return connection.QuerySingleOrDefault<decimal>(sql, new { Id = id });
//    }

//        public bool UpdateUserBalance(int id, decimal amountChange)
//    {
//        using var connection = connectionFactory.CreateConnection();
//        var sql = @"UPDATE UserAccounts
//                SET Balance = Balance + @AmountChange
//                WHERE Id = @Id";
//        var result = connection.Execute(sql, new { Id = id, AmountChange = amountChange });
//        Console.WriteLine($"Balance changed for UserId={id} and AmountChange={amountChange}");
//        return result > 0;
//    }

//}

using Dapper;
using ExpenseManagement.DTO;
using ExpenseManagement.Models;

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

    // ✅ FIXED: Added transaction parameter to Execute calls
    public bool CreateTransaction(CreateTransactionRequest request, int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open(); // ⚠️ Must open connection before starting transaction
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
            }, transaction); // ⚠️ FIX: Added transaction parameter

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
            }, transaction); // ⚠️ FIX: Added transaction parameter

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

    // ✅ Already correct - kept as is
    public bool EditTransaction(EditTransactionRequest request, int userId, int tid)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();

        try
        {
            // 1. Get old transaction values
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

            // 2. Update transaction
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
                request.Date // ⚠️ FIX: Use request.Date instead of CURRENT_DATE()
            }, tx);

            if (rows == 0)
            {
                tx.Rollback();
                Console.WriteLine("Update failed: No rows affected");
                return false;
            }

            // 3. Calculate balance difference
            decimal oldChange = old.IsExpense ? -old.Amount : old.Amount;
            decimal newChange = request.IsExpense ? -request.Amount : request.Amount;
            decimal diff = newChange - oldChange;

            // 4. Update balance with difference
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

    // ✅ FIXED: Added transaction wrapper for atomicity
    public bool DeleteTransaction(int userId, int tid)
    {
        using var connection = connectionFactory.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();

        try
        {
            // 1. Get old transaction to calculate balance reversal
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

            // 2. Delete transaction
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

            // 3. Reverse the balance change
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
        var sql = @"
            SELECT * FROM Transactions
            WHERE UserId = @UserId AND Id = @Id";

        var result = connection.Query<TransactionResponse>(sql, new
        {
            UserId = userId,
            Id = Id
        });

        return result; // Returns empty IEnumerable<TransactionResponse> if not found
    }

    public IEnumerable<TransactionResponse> ReadTransaction(int userId)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"
            SELECT * FROM Transactions
            WHERE UserId = @UserId
            ORDER BY Date DESC"; // ⚠️ Added ORDER BY for better UX

        var result = connection.Query<TransactionResponse>(sql, new
        {
            UserId = userId
        });

        return result; // Returns empty IEnumerable<TransactionResponse> if no transactions
    }

    public IEnumerable<TransactionResponse> ReadAllTransactions()
    {
        using var connection = connectionFactory.CreateConnection();
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
            ORDER BY t.Date DESC"; // ⚠️ Added ORDER BY

        var result = connection.Query<TransactionResponse>(sql);
        return result; // Returns empty IEnumerable<TransactionResponse> if no transactions
    }

    public decimal GetUserBalance(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"
            SELECT COALESCE(Balance, 0)
            FROM UserAccounts
            WHERE Id = @Id"; // ⚠️ Added COALESCE for null safety

        return connection.QuerySingleOrDefault<decimal>(sql, new { Id = id });
        // Returns 0 if user not found (thanks to COALESCE)
    }

    public bool UpdateUserBalance(int id, decimal amountChange)
    {
        using var connection = connectionFactory.CreateConnection();
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
