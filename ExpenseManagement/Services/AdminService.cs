using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface IAdminService
{
    AdminServiceResult ReadAllTransactions();
    AdminServiceResult ReadAllBudgets();
    AdminServiceResult ReadAllReminders();
    public AdminServiceResult<List<UsersDTO>> ViewAllUsers();
}
public class AdminService : IAdminService
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
        var message = result.Any() ? "The transactions are" : "No transactions found";
        return new AdminServiceResult(true, message, result);
    }

    public AdminServiceResult ReadAllBudgets()
    {
        var result = budgetRepository.ReadAllBudgets();
        var message = result.Any() ? "The budgets are" : "No budgets found";
        return new AdminServiceResult(true, message, result);
    }
    public AdminServiceResult ReadAllReminders()
    {
        var result = reminderRepository.ReadAllReminders();
        var message = result.Any() ? "The reminders are" : "No reminders found";
        return new AdminServiceResult(true, message, result);
    }

    public AdminServiceResult<List<UsersDTO>> ViewAllUsers()
    {
        var result = authRepository.ViewAllUsers();
        var users = result.Select(t => new UsersDTO
        {
            Email = t.Email,
            Role = t.Role,
        }).ToList();
        var message = users.Any() ? "The users are" : "No users found";
        return new AdminServiceResult<List<UsersDTO>>(true, message, users);
    }
}

public class AdminServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public ErrorType ErrorType { get; set; }

    public AdminServiceResult(bool success, string message, T? data = default, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        Data = data;
        ErrorType = errorType;
    }
}
public class AdminServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }
    public ErrorType ErrorType { get; set; }

    public AdminServiceResult(bool success, string message, object? data = null, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        Data = data;
        ErrorType = errorType;
    }
}