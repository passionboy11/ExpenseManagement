using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface IAdminService
{
    AdminServiceResult ReadAllTransactions();
    AdminServiceResult  ReadAllBudgets();
    public AdminServiceResult<List<UsersDTO>> ViewAllUsers();
}
public class AdminService:IAdminService
{
    private readonly DataAccess _dataAccess;

    public AdminService(DataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public AdminServiceResult ReadAllTransactions()
    {
        var result = _dataAccess.ReadAllTransactions();
        if (!result.Any())
        {
            return new AdminServiceResult(false,"No transactions found");
        }
        
        return new AdminServiceResult (true,"The transactions are",result);
    }

    public AdminServiceResult ReadAllBudgets()
    {
        var result = _dataAccess.ReadAllBudgets();
        if (!result.Any())
        {
            return new AdminServiceResult(false, "No budgets found");
        }
        return  new AdminServiceResult(true,"The budgets are",result);
    }
   
    public AdminServiceResult<List<UsersDTO>> ViewAllUsers()
    {
        var result = _dataAccess.ViewAllUsers();
        if (!result.Any())
        {
            return new AdminServiceResult<List<UsersDTO>>(false,"No users found");
        }
        var users = result.Select(t=>new UsersDTO
        {
            Email = t.Email,
            Role = t.Role,
        }).ToList();
        return new AdminServiceResult<List<UsersDTO>>(true,"The users are",users);
    }
}

public class AdminServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T?  Data { get; set; }

    public AdminServiceResult(bool success, string message, T? data =default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
public class AdminServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }

    public AdminServiceResult(bool success, string message, object? data = null)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}