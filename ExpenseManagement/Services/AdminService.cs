using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface IAdminService
{
    AdminServiceResult<List<TransactionResponse>> ReadAllTransactions();
}
public class AdminService:IAdminService
{
    private readonly DataAccess _dataAccess;

    public AdminService(DataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }

    public AdminServiceResult<List<TransactionResponse>> ReadAllTransactions()
    {
        var result = _dataAccess.ReadAllTransactions();
        if (!result.Any())
        {
            return new AdminServiceResult<List<TransactionResponse>>(false,"No transactions found");
        }

        var transactions = result.Select(t => new TransactionResponse
        {
            Id = t.Id,
            Amount = t.Amount,
            IsExpense = t.IsExpense,
            Category = t.Category,
            Description = t.Description,
            PaymentMethod = t.PaymentMethod,
            IsRecurring = t.IsRecurring,
            Date = t.Date
        }).ToList();
        return new AdminServiceResult<List<TransactionResponse>>(true,"The transactions are",transactions);
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