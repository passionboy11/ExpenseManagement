using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

//

public interface ITransactionService
{
    TransactionServiceResult CreateTransaction( CreateTransactionRequest request,string email);
    TransactionServiceResult EditTransaction( EditTransactionRequest request,string email);
    TransactionServiceResult DeleteTransaction( DeleteTransactionRequest request,string email);
    TransactionServiceResult <List<TransactionResponse>> ReadTransaction( string email);
}
// decoupling
public class TransactionService: ITransactionService
{
    private readonly DataAccess dataAccess;

    public TransactionService(DataAccess dataAccess)
    {
        this.dataAccess = dataAccess;
    }

    public TransactionServiceResult CreateTransaction(CreateTransactionRequest request,string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult(false, "User not found");
        
        var success = dataAccess.CreateTransaction(request, userId);
        if(!success)
            return new TransactionServiceResult(false, "User creation failed");
        
        return new TransactionServiceResult(true,"Transaction created successfully");
    }

    public TransactionServiceResult EditTransaction(EditTransactionRequest request, string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult(false, "User not found");
        
        var success = dataAccess.EditTransaction(request, userId);
        if (!success)
            return new TransactionServiceResult(false, "User Edit failed");
        
        return new TransactionServiceResult(true,"Edited successfully");
    }

    public TransactionServiceResult DeleteTransaction(DeleteTransactionRequest request,string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult(false, "User not found");
        
        var success = dataAccess.DeleteTransaction(request, userId);
        if (!success)
            return new TransactionServiceResult(false, "User Delete failed");
        
        return new TransactionServiceResult(true,"Transaction deleted successfully");
    }

    public TransactionServiceResult<List<TransactionResponse>> ReadTransaction(string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult<List<TransactionResponse>>(false, "User not found");

        var result  = dataAccess.ReadTransaction(userId);
        if (!result.Any())
        {
            return new TransactionServiceResult<List<TransactionResponse>>(false, "Transaction not found");
        }
        var transactions = result.Select(t => new TransactionResponse
        {
            Id = t.Id,
            Amount = t.Amount,
            Type = t.Type,
            Category = t.Category,
            Description = t.Description,
            PaymentMethod = t.PaymentMethod,
            IsRecurring = t.IsRecurring,
            Date = t.Date
        }).ToList();
        return new TransactionServiceResult<List<TransactionResponse>>(true,"Transactions read successfully",transactions);
    }
}
public class TransactionServiceResult
{
    public bool Success { get; }
    public string Message { get; }
    public TransactionServiceResult(bool success, string message) 
    {
            Success = success;
            Message = message; 
    }
}
public class TransactionServiceResult<T>
{
    public bool Success { get; }
    public string Message { get; }
    
    public T? Data { get; }
    public TransactionServiceResult(bool success, string message, T? data = default) 
    {
        Success = success;
        Message = message;
        Data = data;
    }

}