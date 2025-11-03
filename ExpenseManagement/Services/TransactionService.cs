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
    TransactionServiceResult <BalanceResponse> GetBalance( string email);
}
// decoupling
public class TransactionService: ITransactionService
{
    private readonly DataAccess dataAccess;

    public TransactionService(DataAccess dataAccess)
    {
        this.dataAccess = dataAccess;
    }

    public TransactionServiceResult CreateTransaction(CreateTransactionRequest request, string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult(false, "User not found");

        var success = dataAccess.CreateTransaction(request, userId);
        if (!success)
            return new TransactionServiceResult(false, "Transaction creation failed");

        decimal amountChange = request.IsExpense ? -request.Amount : request.Amount;
        var balanceUpdated = dataAccess.UpdateUserBalance(userId, amountChange);
        if (!balanceUpdated)
            return new TransactionServiceResult(false, "Failed to update balance");

        string? alertMessage = null;

        // Only check budget limit for expense transactions
        if (request.IsExpense)
        {
            var totalExpenses = dataAccess.GetBudgetUsage(userId, request.Category);
            var budgetLimit = dataAccess.GetBudgetLimit(userId, request.Category);

            if (budgetLimit > 0)
            {
                decimal usagePercent = (totalExpenses / budgetLimit) * 100;
                if (usagePercent >= 80)
                {
                    alertMessage = $"⚠️ Warning: You’ve reached {usagePercent:F0}% of your budget for {request.Category}.";
                }
            }
        }

        return new TransactionServiceResult(
            true,
            "Transaction created and balance updated successfully",
            alertMessage
        );
    }


    public TransactionServiceResult EditTransaction(EditTransactionRequest request, string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult(false, "User not found");
        
        var success = dataAccess.EditTransaction(request, userId);
        if (!success)
            return new TransactionServiceResult(false, "Transaction Edit failed");
        
        return new TransactionServiceResult(true,"Edited successfully");
    }

    public TransactionServiceResult DeleteTransaction(DeleteTransactionRequest request,string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult(false, "User not found");
        
        var success = dataAccess.DeleteTransaction(request, userId);
        if (!success)
            return new TransactionServiceResult(false, "Transaction Delete failed");
        
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
            IsExpense = t.IsExpense,
            Category = t.Category,
            Description = t.Description,
            PaymentMethod = t.PaymentMethod,
            IsRecurring = t.IsRecurring,
            Date = t.Date
        }).ToList();
        return new TransactionServiceResult<List<TransactionResponse>>(true,"Transactions read successfully",transactions);
    }

    public TransactionServiceResult<BalanceResponse> GetBalance(string email)
    {
        var userId = dataAccess.FindUserIdByEmail(email);
        if (userId is 0)
            return new TransactionServiceResult<BalanceResponse>(false, "User not found");

        // Suppose this returns decimal
        var balanceAmount = dataAccess.GetUserBalance(userId); 
        return new TransactionServiceResult<BalanceResponse>(
            true, 
            "Balance retrieved successfully", 
            new BalanceResponse { Amount = balanceAmount }
        );
    }
    
}
public class TransactionServiceResult
{
    public bool Success { get; }
    public string Message { get; }
    public string? Alert { get; }
    public TransactionServiceResult(bool success, string message,string? alert=null) 
    {
            Success = success;
            Message = message; 
            Alert = alert;
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