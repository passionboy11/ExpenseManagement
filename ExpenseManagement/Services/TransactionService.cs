//using ExpenseManagement.DTO;
//using ExpenseManagement.Infrastructure;


//namespace ExpenseManagement.Services;

////

//public interface ITransactionService
//{
//    TransactionServiceResult CreateTransaction( CreateTransactionRequest request,int userId);
//    TransactionServiceResult EditTransaction( EditTransactionRequest request,int userId,int tid);
//    TransactionServiceResult DeleteTransaction(int userId,int tid);
//    TransactionServiceResult <List<TransactionResponse>> ReadTransaction( int userId);
//    TransactionServiceResult<TransactionResponse> GetTransactionById(int id,int tid);
//    TransactionServiceResult <BalanceResponse> GetBalance(int userId);
//}
//// decoupling
//public class TransactionService: ITransactionService
//{
//    private readonly ITransactionRepository transactionRepository;
//    private readonly IBudgetRepository budgetRepository;

//    public TransactionService(ITransactionRepository transactionRepository, IBudgetRepository budgetRepository)
//    {
//        this.transactionRepository = transactionRepository;
//        this.budgetRepository = budgetRepository;
//    }

//    public TransactionServiceResult CreateTransaction(CreateTransactionRequest request, int userId)
//    {
//        var success = transactionRepository.CreateTransaction(request, userId);
//        if (!success)
//            return new TransactionServiceResult(false, "Transaction creation failed");

//        return new TransactionServiceResult(
//            true,
//            "Transaction created successfully"
//        );
//    }


//    public TransactionServiceResult EditTransaction(EditTransactionRequest request, int userId, int tid)
//    {
//        try
//        {
//            var success = transactionRepository.EditTransaction(request, userId, tid);

//            if (!success)
//                return new TransactionServiceResult(false, "No transaction was updated");

//            return new TransactionServiceResult(true, "Transaction edited successfully");
//        }
//        catch (Exception ex)
//        {
//            return new TransactionServiceResult(false, $"Database error: {ex.Message}");
//        }
//    }


//    public TransactionServiceResult DeleteTransaction(int userId, int tid)
//    {

//        var success = transactionRepository.DeleteTransaction(userId,tid);
//        if (!success)
//            return new TransactionServiceResult(false, "Transaction Delete failed");

//        return new TransactionServiceResult(true,"Transaction deleted successfully");
//    }

//    public TransactionServiceResult<List<TransactionResponse>> ReadTransaction(int userId)
//    {
//        var result  = transactionRepository.ReadTransaction(userId);
//        if (!result.Any())
//        {
//            return new TransactionServiceResult<List<TransactionResponse>>(false, "Transaction not found");
//        }
//        var transactions = result.Select(t => new TransactionResponse
//        {
//            Id = t.Id,
//            Amount = t.Amount,
//            IsExpense = t.IsExpense,
//            Category = t.Category,
//            Description = t.Description,
//            PaymentMethod = t.PaymentMethod,
//            IsRecurring = t.IsRecurring,
//            Date = t.Date
//        }).ToList();
//        return new TransactionServiceResult<List<TransactionResponse>>(true,"Transactions read successfully",transactions);
//    }

//    public TransactionServiceResult<TransactionResponse> GetTransactionById(int userId,int tid)
//    {
//        var result = transactionRepository.GetTransactionById(userId,tid);
//        if (!result.Any())
//        {
//            return new TransactionServiceResult<TransactionResponse>(false, "Transaction not found");
//        }
//        var transaction = result.Select(t => new TransactionResponse
//        {
//            Id = t.Id,
//            Amount = t.Amount,
//            IsExpense = t.IsExpense,
//            Category = t.Category,
//            Description = t.Description,
//            PaymentMethod = t.PaymentMethod,
//            IsRecurring = t.IsRecurring,
//            Date = t.Date
//        }).FirstOrDefault();
//        return new TransactionServiceResult<TransactionResponse>(true,"Transaction shown",transaction);

//    }

//    public TransactionServiceResult<BalanceResponse> GetBalance(int userId)
//    {
//        // Suppose this returns decimal
//        var balanceAmount = transactionRepository.GetUserBalance(userId); 
//        return new TransactionServiceResult<BalanceResponse>(
//            true, 
//            "Balance retrieved successfully", 
//            new BalanceResponse { Amount = balanceAmount }
//        );
//    }

//}
//public class TransactionServiceResult
//{
//    public bool Success { get; }
//    public string Message { get; }
//    public TransactionServiceResult(bool success, string message) 
//    {
//            Success = success;
//            Message = message; 
//    }
//}
//public class TransactionServiceResult<T>
//{
//    public bool Success { get; }
//    public string Message { get; }

//    public T? Data { get; }
//    public TransactionServiceResult(bool success, string message, T? data = default) 
//    {
//        Success = success;
//        Message = message;
//        Data = data;
//    }

//}
using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface ITransactionService
{
    TransactionServiceResult CreateTransaction(CreateTransactionRequest request, int userId);
    TransactionServiceResult EditTransaction(EditTransactionRequest request, int userId, int tid);
    TransactionServiceResult DeleteTransaction(int userId, int tid);
    TransactionServiceResult<List<TransactionResponse>> ReadTransaction(int userId);
    TransactionServiceResult<TransactionResponse> GetTransactionById(int id, int tid);
    TransactionServiceResult<BalanceResponse> GetBalance(int userId);
}

public class TransactionService : ITransactionService
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
        try
        {
            var success = transactionRepository.CreateTransaction(request, userId);
            if (!success)
                return new TransactionServiceResult(false, "Failed to create transaction. Please try again.");

            return new TransactionServiceResult(true, "Transaction created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateTransaction error: {ex.Message}");
            return new TransactionServiceResult(false, "An error occurred while creating the transaction");
        }
    }

    public TransactionServiceResult EditTransaction(EditTransactionRequest request, int userId, int tid)
    {
        try
        {
            var success = transactionRepository.EditTransaction(request, userId, tid);

            if (!success)
                return new TransactionServiceResult(false, "Transaction not found or update failed");

            return new TransactionServiceResult(true, "Transaction updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EditTransaction error: {ex.Message}");
            return new TransactionServiceResult(false, "An error occurred while updating the transaction");
        }
    }

    public TransactionServiceResult DeleteTransaction(int userId, int tid)
    {
        try
        {
            var success = transactionRepository.DeleteTransaction(userId, tid);
            if (!success)
                return new TransactionServiceResult(false, "Transaction not found or already deleted");

            return new TransactionServiceResult(true, "Transaction deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteTransaction error: {ex.Message}");
            return new TransactionServiceResult(false, "An error occurred while deleting the transaction");
        }
    }

    public TransactionServiceResult<List<TransactionResponse>> ReadTransaction(int userId)
    {
        try
        {
            var result = transactionRepository.ReadTransaction(userId);

            // Empty list is valid - user has no transactions yet (200 OK with empty array)
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

            var message = transactions.Any()
                ? "Transactions retrieved successfully"
                : "No transactions found";

            return new TransactionServiceResult<List<TransactionResponse>>(true, message, transactions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReadTransaction error: {ex.Message}");
            return new TransactionServiceResult<List<TransactionResponse>>(false, "An error occurred while retrieving transactions");
        }
    }

    public TransactionServiceResult<TransactionResponse> GetTransactionById(int userId, int tid)
    {
        try
        {
            var result = transactionRepository.GetTransactionById(userId, tid);
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

            return new TransactionServiceResult<TransactionResponse>(true, "Transaction retrieved successfully", transaction!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetTransactionById error: {ex.Message}");
            return new TransactionServiceResult<TransactionResponse>(false, "An error occurred while retrieving the transaction");
        }
    }

    public TransactionServiceResult<BalanceResponse> GetBalance(int userId)
    {
        try
        {
            var balanceAmount = transactionRepository.GetUserBalance(userId);
            return new TransactionServiceResult<BalanceResponse>(
                true,
                "Balance retrieved successfully",
                new BalanceResponse { Amount = balanceAmount }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetBalance error: {ex.Message}");
            return new TransactionServiceResult<BalanceResponse>(false, "An error occurred while retrieving the balance");
        }
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
