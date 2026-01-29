using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Runtime.InteropServices;

namespace ExpenseManagement.Infrastructure;
public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
    
}
public class DbConnectionFactory:IDbConnectionFactory
{
    private readonly IConfiguration configuration;

    public DbConnectionFactory(IConfiguration configuration)
    {
     this.configuration = configuration;
    }
    public MySqlConnection CreateConnection()
    {
        return new MySqlConnection(
            configuration.GetConnectionString("DefaultConnection"));
    }
}




