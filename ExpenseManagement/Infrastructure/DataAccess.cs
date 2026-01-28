using Dapper;
using ExpenseManagement.DTO;
using ExpenseManagement.Models;
using MySqlConnector;
using Budget = ExpenseManagement.Models.Budget;

namespace ExpenseManagement.Infrastructure
{

    public class DataAccess : IDisposable
    {
        private MySqlConnection connection;

        public DataAccess(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        public void Dispose()
        {
                connection.Dispose();
        }

        public bool RegisterUser(string email, string password, string role)
        {
            // Execute Scalar returns a single value the count
            // If count is 0, it means email already exists -> returns false

            var accountCount = connection.ExecuteScalar<int>(
                "SELECT Count(1) FROM UserAccounts WHERE Email = @email",
                new { email }
            );

            if (accountCount > 0) return false;

            var sql = "INSERT INTO UserAccounts (Email, Password,Role) VALUES (@email, @password,@role)";
            var result = connection.Execute(sql, new { email, password,role});

            return result > 0;
        }

        public int FindUserIdByEmail(string email)
        {
            var sql = "SELECT Id from UserAccounts where Email = @email";
            return connection.QueryFirstOrDefault<int>(sql, new { email });
        }
        public UserAccount? FindUserByEmail(string email)
        {
            var sql = "SELECT * FROM UserAccounts WHERE Email = @email";

            return connection.QueryFirstOrDefault<UserAccount>(sql, new { email = email });
        }

        //InsertRegreshtToken( RefreshToken, email)

        public bool InsertRefreshtoken(RefreshToken refreshToken, string email)
        {
            var sql =
                "INSERT INTO RefreshToken (Token, CreatedDate, Expires, Enabled, Email) VALUES (@token, @createdDate, @expires, @enabled, @email)";

            var result = connection.Execute(sql, new
            {
                refreshToken.Token,
                refreshToken.CreatedDate,
                refreshToken.Expires,
                refreshToken.Enabled,
                Email = email
            });
            return result > 0;
        }

        // DiableUserTokensByEmail(string email)
        public bool DisableUserTokenByEmail(string email)
        {
            var sql = "UPDATE RefreshToken SET Enabled = 0 WHERE Email = @email";

            var result = connection.Execute(sql, new { Email = email });
            return result > 0;
        }

        // DisableUserToken (string token)
        public bool DisableUserToken(string token)
        {
            var sql = "UPDATE RefreshToken SET Enabled = 0 WHERE Token = @token";
            var result = connection.Execute(sql, new { Token = token });
            return result > 0;
        }
        // IsRefreshTokenValid(strin token)

        public bool IsRefreshTokenValid(string token)
        {
            var sql = "SELECT COUNT(1) FROM RefreshToken WHERE Token = @token AND Enabled = 1 AND Expires >= CURDATE()";

            var result = connection.ExecuteScalar<int>(sql, new { Token = token });
            return result > 0;
        }

        // FindUesrByToken(string token)
        public UserAccount? FindUserByToken(string token)
        {
            var sql =
                "SELECT UserAccounts.* FROM RefreshToken JOIN UserAccounts ON RefreshToken.Email = UserAccounts.Email WHERE Token= @token";
            return connection.QueryFirstOrDefault<UserAccount>(sql, new { Token = token });
        }

        public bool CreateTransaction(CreateTransactionRequest request, int userId)
        {
            var sql =
                "INSERT INTO Transactions (UserId,Amount,IsExpense,Category,Description,PaymentMethod,IsRecurring,Date) VALUES (@UserId,@Amount,@IsExpense,@Category,@Description,@PaymentMethod,@IsRecurring,@Date)";
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
            });
            Console.WriteLine($"Rows affected: {result}");
            decimal change = request.IsExpense ? -request.Amount : request.Amount;
            UpdateUserBalance(userId, change);
            return result > 0;
        }

        public bool EditTransaction(EditTransactionRequest request, int userId, int tid)
        {
            var old = connection.QuerySingleOrDefault<Transaction>(
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
            return result >0;
        }

        public bool DeleteTransaction( int userId,int tid)
        {
            var old = connection.QuerySingleOrDefault<Transaction>(
                "SELECT Amount, IsExpense FROM Transactions WHERE Id = @Tid AND UserId = @UserId",
                new { Tid = tid, UserId = userId }
            );
            var sql = @"DELETE FROM Transactions WHERE  UserId = @UserId AND Id = @Tid";
            var result = connection.Execute(sql,new
            {
                UserId = userId,
                Tid = tid,
            });
            Console.WriteLine($"Deleting Transaction Id={tid} for UserId={userId}");
            Console.WriteLine($"Rows affected: {result}");
            
            if(result > 0 && old != null)
            {
                decimal change = old.IsExpense ? old.Amount : -old.Amount; // Reverse effect
                UpdateUserBalance(userId, -change); // reverse old transaction
            }

            return result >0;

        }   

        public IEnumerable<Transaction> ReadTransaction( int  userId)
        {
            var sql = @"SELECT * FROM Transactions WHERE UserId = @UserId";

            var result = connection.Query<Transaction>(sql, new
            {
                UserId = userId,
            });

            return result;
        }

        public IEnumerable<Transaction> GetTransactionById(int userId, int Id)
        {
            var sql ="SELECT * FROM TRANSACTIONS WHERE UserId =@userId AND Id = @Id";
            var result = connection.Query<Transaction>(sql, new
            {
                userId = userId, 
                Id = Id
            });
            return result;
        }

        public IEnumerable<TransactionResponse> ReadAllTransactions()
        {
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

        public bool CreateBudget(ExpenseManagement.DTO.Budget budget, int userId)
        {
            if(userId == 0)
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

        public bool EditBudget(ExpenseManagement.DTO.EditBudget budget, int userId)
        {
            var sql = @"UPDATE Budgets SET Category = @Category, LimitAmount = @LimitAmount, MonthYear = @MonthYear WHERE UserId = @UserId AND Id = @Id";

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

        public bool DeleteBudget(ExpenseManagement.DTO.DeleteBudget budget, int userId)
        {
            var sql =  @"DELETE FROM Budgets WHERE  UserId = @UserId AND Id = @Id";
            var result = connection.Execute(sql, new
            {
                budget.Id,
                UserID = userId
            });
            return result > 0;
        }

        public IEnumerable<BudgetResponse> ReadBudget(int userId)
        {
            var sql = @"SELECT b.*, u.Email FROM Budgets b INNER JOIN UserAccounts u on b.UserId =u.Id WHERE b.UserId = @UserId";
            var result = connection.Query<BudgetResponse>(sql, new
            {
                UserId = userId
            });
            return result;
        }

        public IEnumerable<BudgetResponse> ReadAllBudgets()
        {
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

        public IEnumerable<UserAccount> ViewAllUsers()
        {
            var sql =@"SELECT * FROM  UserAccounts";
            var result = connection.Query<UserAccount>(sql);
            return result;
        }
        public decimal GetUserBalance(int id)
        {
            var sql = @"SELECT Balance FROM UserAccounts WHERE Id = @Id";
            return connection.QuerySingleOrDefault<decimal>(sql, new { Id = id });
        }

        public bool UpdateUserBalance(int id, decimal amountChange)
        {
            var sql = @"UPDATE UserAccounts
                SET Balance = Balance + @AmountChange
                WHERE Id = @Id";
            var result = connection.Execute(sql, new { Id = id, AmountChange = amountChange });
            Console.WriteLine($"Balance changed for UserId={id} and AmountChange={amountChange}");
            return result > 0;
        }

        public decimal GetBudgetUsage(int userId, string category)
        {
            var sql = @"SELECT IFNULL(SUM(Amount), 0)
                FROM Transactions
                WHERE UserId = @UserId AND IsExpense = 1 AND Category = @Category";
            return connection.QuerySingleOrDefault<decimal>(sql, new { UserId = userId, Category = category });
        }

        public decimal GetBudgetLimit(int userId, string category)
        {
            var sql = @"SELECT LimitAmount
                FROM Budgets
                WHERE UserId = @UserId AND Category = @Category";
            return connection.QuerySingleOrDefault<decimal>(sql, new { UserId = userId, Category = category });
        }
        
    }
}