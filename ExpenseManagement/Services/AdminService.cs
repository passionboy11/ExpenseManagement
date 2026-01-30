using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface IAdminService
{
    AdminServiceResult ReadAllTransactions();
    AdminServiceResult  ReadAllBudgets();
    AdminServiceResult ReadAllReminders();
    public AdminServiceResult<List<UsersDTO>> ViewAllUsers();
}
public class AdminService:IAdminService
{
    private readonly IBudgetRepository budgetRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IAuthRepository authRepository;
    private readonly IReminderRepository reminderRepository;

    public AdminService(ITransactionRepository transactionRepository, IBudgetRepository budgetRepository, IAuthRepository authRepository, IReminderRepository reminderRepository)
    {
        this.transactionRepository = transactionRepository;
        this.budgetRepository = budgetRepository;
        this.authRepository = authRepository;
        this.reminderRepository = reminderRepository;
    }

    public AdminServiceResult ReadAllTransactions()
    {
        var result = transactionRepository.ReadAllTransactions();
        if (!result.Any())
        {
            return new AdminServiceResult(false,"No transactions found");
        }
        
        return new AdminServiceResult (true,"The transactions are",result);
    }

    public AdminServiceResult ReadAllBudgets()
    {
        var result = budgetRepository.ReadAllBudgets();
        if (!result.Any())
        {
            return new AdminServiceResult(false, "No budgets found");
        }
        return  new AdminServiceResult(true,"The budgets are",result);
    }
    public AdminServiceResult ReadAllReminders()
    {
        var result = reminderRepository.ReadAllReminders();
        if (!result.Any())
        {
            return new AdminServiceResult(false, "No budgets found");
        }
        return new AdminServiceResult(true, "The budgets are", result);
    }
   
    public AdminServiceResult<List<UsersDTO>> ViewAllUsers()
    {
        var result = authRepository.ViewAllUsers();
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