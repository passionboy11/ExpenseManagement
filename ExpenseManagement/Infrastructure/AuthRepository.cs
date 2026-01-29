using Dapper;
using ExpenseManagement.Models;

namespace ExpenseManagement.Infrastructure;
public interface IAuthRepository
{
    bool RegisterUser(string email, string password, string role);
    int FindUserIdByEmail(string email);
    UserAccount? FindUserByEmail(string email);
    bool InsertRefreshtoken(RefreshToken refreshToken, string email);
    bool DisableUserTokenByEmail(string email);
    bool DisableUserToken(string token);
    bool IsRefreshTokenValid(string token);
    UserAccount? FindUserByToken(string token);
    IEnumerable<UserAccount> ViewAllUsers();


}
    public class AuthRepository:IAuthRepository
    {
        private readonly IDbConnectionFactory connectionFactory;
        public AuthRepository(IDbConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }
        public bool RegisterUser(string email, string password, string role)
        {
          
            using var connection =connectionFactory.CreateConnection();
            var accountCount =  connection.ExecuteScalar<int>(
                "SELECT Count(1) FROM UserAccounts WHERE Email = @email",
                new { email }
            );

            if (accountCount > 0) return false;

            var sql = "INSERT INTO UserAccounts (Email, Password,Role) VALUES (@email, @password,@role)";
            var result = connection.Execute(sql, new { email, password, role });

            return result > 0;
        }
        public int FindUserIdByEmail(string email)
        {
            using var connection = connectionFactory.CreateConnection();
            var sql = "SELECT Id from UserAccounts where Email = @email";
            return connection.QueryFirstOrDefault<int>(sql, new { email });
        }
        public UserAccount? FindUserByEmail(string email)
        {
            using var connection = connectionFactory.CreateConnection();
            var sql = "SELECT * FROM UserAccounts WHERE Email = @email";

            return connection.QueryFirstOrDefault<UserAccount>(sql, new { email = email });
        }
        public bool InsertRefreshtoken(RefreshToken refreshToken, string email)
        {
            using var connection = connectionFactory.CreateConnection();
        connection.Open();
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
        public bool DisableUserTokenByEmail(string email)
        {
            using var connection = connectionFactory.CreateConnection();
            var sql = "UPDATE RefreshToken SET Enabled = 0 WHERE Email = @email";

            var result = connection.Execute(sql, new { Email = email });
            return result > 0;
        }
        public bool DisableUserToken(string token)
        {
            using var connection = connectionFactory.CreateConnection();
            var sql = "UPDATE RefreshToken SET Enabled = 0 WHERE Token = @token";
            var result = connection.Execute(sql, new { Token = token });
            return result > 0;
        }
        public bool IsRefreshTokenValid(string token)
        {
            using var connection = connectionFactory.CreateConnection();
            var sql = "SELECT COUNT(1) FROM RefreshToken WHERE Token = @token AND Enabled = 1 AND Expires >= CURDATE()";

            var result = connection.ExecuteScalar<int>(sql, new { Token = token });
            return result > 0;
        }
        public UserAccount? FindUserByToken(string token)
        {
            using var connection = connectionFactory.CreateConnection();
            var sql =
                "SELECT UserAccounts.* FROM RefreshToken JOIN UserAccounts ON RefreshToken.Email = UserAccounts.Email WHERE Token= @token";
            return connection.QueryFirstOrDefault<UserAccount>(sql, new { Token = token });
        }
    public IEnumerable<UserAccount> ViewAllUsers()
    {
        using var connection = connectionFactory.CreateConnection();
        var sql = @"SELECT * FROM  UserAccounts";
        var result = connection.Query<UserAccount>(sql);
        return result;
    }
}

