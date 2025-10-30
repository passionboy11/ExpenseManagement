using Dapper;
using ExpenseManagement.DTO;
using ExpenseManagement.Models;
using MySqlConnector;

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
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
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

            var sql = "INSERT INTO UserAccounts (Email, Password, Role) VALUES (@email, @password, @role)";
            var result = connection.Execute(sql, new { email, password, role });

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
                "INSERT INTO Transactions (UserId,Amount,Type,Category,Description,PaymentMethod,IsRecurring,Date) VALUES (@UserId,@Amount,@Type,@Category,@Description,@PaymentMethod,@IsRecurring,@Date)";
            var result = connection.Execute(sql, new
            {
                UserId = userId,
                request.Amount,
                request.Type,
                request.Category,
                request.Description,
                request.PaymentMethod,
                request.IsRecurring,
                request.Date
            });
            Console.WriteLine($"Rows affected: {result}");
            return result > 0;
        }

        public bool EditTransaction(EditTransactionRequest request, int userId)
        {
            var sql = @"UPDATE Transactions SET Amount = @Amount, 
                                                Type= @Type, 
                                                Category = @Category, 
                                                PaymentMethod = @PaymentMethod, 
                                                Description = @Description,
                                                IsRecurring = @IsRecurring,
                                                Date=CURRENT_DATE() 
                                                WHERE UserId = @UserId AND Id = @Id";
            var result = connection.Execute(sql, new
            {
                Id = request.Id,
                UserId = userId,
                request.Amount,
                request.Type,
                request.Category,
                request.Description,
                request.PaymentMethod,
                request.IsRecurring,
                
            });
            return result > 0;
        }

        public bool DeleteTransaction(DeleteTransactionRequest request, int userId)
        {
            var sql = @"DELETE FROM Transactions WHERE  UserId = @UserId AND Id = @Id";
            var result = connection.Execute(sql,new
            {
                Id = request.Id,
                UserId = userId,
            });
            Console.WriteLine($"Deleting Transaction Id={request.Id} for UserId={userId}");
            Console.WriteLine($"Rows affected: {result}");


            return result > 0;
            
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
    }
}