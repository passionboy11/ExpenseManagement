using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;


namespace ExpenseManagement.Services;

//

public interface ITransactionService
{
    TransactionServiceResult CreateTransaction( CreateTransactionRequest request,int userId);
    TransactionServiceResult EditTransaction( EditTransactionRequest request,int userId,int tid);
    TransactionServiceResult DeleteTransaction(int userId,int tid);
    TransactionServiceResult <List<TransactionResponse>> ReadTransaction( int userId);
    TransactionServiceResult<TransactionResponse> GetTransactionById(int id,int tid);
    TransactionServiceResult <BalanceResponse> GetBalance(int userId);
}
// decoupling
public class TransactionService: ITransactionService
{
    private readonly ITransactionRepository transactionRepository;
    private readonly IBudgetRepository budgetRepository;

    public TransactionService(ITransactionRepository transactionRepository, IBudgetRepository budgetRepository)
    {
        this.transactionRepository = transactionRepository;
        this.budgetRepository = budgetRepository;
    }

    public TransactionServiceResult CreateTransaction(CreateTransactionRequest request, int userId)
    {


        var success = transactionRepository.CreateTransaction(request, userId);
        if (!success)
            return new TransactionServiceResult(false, "Transaction creation failed");
//// remove this 
        decimal amountChange = request.IsExpense ? -request.Amount : request.Amount;
        var balanceUpdated = transactionRepository.UpdateUserBalance(userId, amountChange);
        if (!balanceUpdated)
            return new TransactionServiceResult(false, "Failed to update balance");

        string? alertMessage = null;

        // Only check budget limit for expense transactions
        if (request.IsExpense)
        {
            var totalExpenses = budgetRepository.GetBudgetUsage(userId, request.Category);
            var budgetLimit = budgetRepository.GetBudgetLimit(userId, request.Category);

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


    public TransactionServiceResult EditTransaction(EditTransactionRequest request, int userId, int tid)
    {
        try
        {
            var success = transactionRepository.EditTransaction(request, userId, tid);

            if (!success)
                return new TransactionServiceResult(false, "No transaction was updated");

            return new TransactionServiceResult(true, "Transaction edited successfully");
        }
        catch (Exception ex)
        {
            return new TransactionServiceResult(false, $"Database error: {ex.Message}");
        }
    }


    public TransactionServiceResult DeleteTransaction(int userId, int tid)
    {
        
        var success = transactionRepository.DeleteTransaction(userId,tid);
        if (!success)
            return new TransactionServiceResult(false, "Transaction Delete failed");
        
        return new TransactionServiceResult(true,"Transaction deleted successfully");
    }

    public TransactionServiceResult<List<TransactionResponse>> ReadTransaction(int userId)
    {
        var result  = transactionRepository.ReadTransaction(userId);
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

    public TransactionServiceResult<TransactionResponse> GetTransactionById(int userId,int tid)
    {
        var result = transactionRepository.GetTransactionById(userId,tid);
        if (!result.Any())
        {
            return new TransactionServiceResult<TransactionResponse>(false, "Transaction not found");
        }
        var transaction = result.Select(t => new TransactionResponse
        {
            Id = t.Id,
            Amount = t.Amount,
            IsExpense = t.IsExpense,
            Category = t.Category,
            Description = t.Description,
            PaymentMethod = t.PaymentMethod,
            IsRecurring = t.IsRecurring,
            Date = t.Date
        }).FirstOrDefault();
        return new TransactionServiceResult<TransactionResponse>(true,"Transaction shown",transaction);
        
    }

    public TransactionServiceResult<BalanceResponse> GetBalance(int userId)
    {
        // Suppose this returns decimal
        var balanceAmount = transactionRepository.GetUserBalance(userId); 
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