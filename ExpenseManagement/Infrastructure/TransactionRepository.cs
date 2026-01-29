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


public class TransactionRepository:ITransactionRepository
    {
        private readonly IDbConnectionFactory connectionFactory;
        public TransactionRepository(IDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }
        public bool CreateTransaction(CreateTransactionRequest request, int userId)
        {
            using var connection = connectionFactory.CreateConnection();
            var sql =
                "INSERT INTO Transactions (UserId,Amount,IsExpense,Category,Description,PaymentMethod,IsRecurring,Date) VALUES (@UserId,@Amount,@IsExpense,@Category,@Description,@PaymentMethod,@IsRecurring,@Date)";
            var result = connection.Execute(sql, new
            {   /// transaction rolllback insert update delete
                UserId = userId,
                request.Amount,
                request.IsExpense,
                request.Category,
                request.Description,
                request.PaymentMethod,
                request.IsRecurring,
                request.Date
            });
            Console.WriteLine($"Rows affected: {result}");
            decimal change = request.IsExpense ? -request.Amount : request.Amount;
            UpdateUserBalance(userId, change);
            return result > 0;
        }

    public bool EditTransaction(EditTransactionRequest request, int userId, int tid)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var old = connection.QuerySingleOrDefault<TransactionResponse>(
                    "SELECT Amount, IsExpense FROM Transactions WHERE Id = @Tid AND UserId = @UserId",
                    new { Tid = tid, UserId = userId }
                );

            var sql = @"UPDATE Transactions SET Amount = @Amount, 
                                                IsExpense= @IsExpense, 
                                                Category = @Category, 
                                                PaymentMethod = @PaymentMethod, 
                                                Description = @Description,
                                                IsRecurring = @IsRecurring,
                                                Date=CURRENT_DATE() 
                                                WHERE UserId = @UserId AND Id = @tid";
            var result = connection.Execute(sql, new
            {
                Tid = tid,
                request.Id,
                UserId = userId,
                request.Amount,
                request.IsExpense,
                request.Category,
                request.Description,
                request.PaymentMethod,
                request.IsRecurring,

            });
            if (result > 0 && old != null)
            {
                // Subtract old value, add new value
                decimal oldChange = old.IsExpense ? -old.Amount : old.Amount;
                decimal newChange = request.IsExpense ? -request.Amount : request.Amount;
                decimal diff = newChange - oldChange;
                UpdateUserBalance(userId, diff);
            }
            return result > 0;
        }
        catch (MySqlConnector.MySqlException ex)
        {
            Console.WriteLine($"MySQL Error: {ex.Message}");
            return false;
        }
    }

        public bool DeleteTransaction(int userId, int tid)
        {
        using var connection = connectionFactory.CreateConnection();
        var old = connection.QuerySingleOrDefault<TransactionResponse>(
                "SELECT Amount, IsExpense FROM Transactions WHERE Id = @Tid AND UserId = @UserId",
                new { Tid = tid, UserId = userId }
            );
            var sql = @"DELETE FROM Transactions WHERE  UserId = @UserId AND Id = @Tid";
            var result = connection.Execute(sql, new
            {
                UserId = userId,
                Tid = tid,
            });
            Console.WriteLine($"Deleting Transaction Id={tid} for UserId={userId}");
            Console.WriteLine($"Rows affected: {result}");

            if (result > 0 && old != null)
            {
                decimal change = old.IsExpense ? old.Amount : -old.Amount; // Reverse effect
                UpdateUserBalance(userId, -change); // reverse old transaction
            }

            return result > 0;

        }

        public IEnumerable<TransactionResponse> ReadTransaction(int userId)
        {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"SELECT * FROM Transactions WHERE UserId = @UserId";

            var result = connection.Query<TransactionResponse>(sql, new
            {
                UserId = userId,
            });

            return result;
        }

        public IEnumerable<TransactionResponse> GetTransactionById(int userId, int Id)
        {
        using var connection = connectionFactory.CreateConnection();
        var sql = "SELECT * FROM Transactions WHERE UserId =@userId AND Id = @Id";
            var result = connection.Query<TransactionResponse>(sql, new
            {
                userId = userId,
                Id = Id
            });
            return result;
        }

        public IEnumerable<TransactionResponse> ReadAllTransactions()
        {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"SELECT t.Id,
                               u.Email,
                               t.Amount,
                               t.IsExpense,
                               t.Category,
                               t.Description,
                               t.PaymentMethod,
                               t.IsRecurring,
                               t.Date
                    FROM Transactions t INNER JOIN UserAccounts u ON t.UserId = u.Id";
            var result = connection.Query<TransactionResponse>(sql);
            return result;
        }
        public decimal GetUserBalance(int id)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"SELECT Balance FROM UserAccounts WHERE Id = @Id";
        return connection.QuerySingleOrDefault<decimal>(sql, new { Id = id });
    }

        public bool UpdateUserBalance(int id, decimal amountChange)
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"UPDATE UserAccounts
                SET Balance = Balance + @AmountChange
                WHERE Id = @Id";
        var result = connection.Execute(sql, new { Id = id, AmountChange = amountChange });
        Console.WriteLine($"Balance changed for UserId={id} and AmountChange={amountChange}");
        return result > 0;
    }

}

