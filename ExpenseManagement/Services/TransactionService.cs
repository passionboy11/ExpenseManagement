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
    private readonly ILogger<TransactionService> logger;

    public TransactionService(ITransactionRepository transactionRepository, IBudgetRepository budgetRepository, ILogger<TransactionService> logger)
    {
        this.transactionRepository = transactionRepository;
        this.budgetRepository = budgetRepository;
        this.logger = logger;
    }

    public TransactionServiceResult CreateTransaction(CreateTransactionRequest request, int userId)
    {
        try
        {
            var success = transactionRepository.CreateTransaction(request, userId);
            if (!success)
                return new TransactionServiceResult(false, "Failed to create transaction. Please try again.", ErrorType.Validation);

            return new TransactionServiceResult(true, "Transaction created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateTransaction error for user {UserId}", userId);
            return new TransactionServiceResult(false, "An error occurred while creating the transaction", ErrorType.ServerError);
        }
    }

    public TransactionServiceResult EditTransaction(EditTransactionRequest request, int userId, int tid)
    {
        try
        {
            var success = transactionRepository.EditTransaction(request, userId, tid);

            if (!success)
                return new TransactionServiceResult(false, "Transaction not found or update failed", ErrorType.NotFound);

            return new TransactionServiceResult(true, "Transaction updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EditTransaction error for user {UserId}, transaction {TransactionId}", userId, tid);
            return new TransactionServiceResult(false, "An error occurred while updating the transaction", ErrorType.ServerError);
        }
    }

    public TransactionServiceResult DeleteTransaction(int userId, int tid)
    {
        try
        {
            var success = transactionRepository.DeleteTransaction(userId, tid);
            if (!success)
                return new TransactionServiceResult(false, "Transaction not found or already deleted", ErrorType.NotFound);

            return new TransactionServiceResult(true, "Transaction deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteTransaction error for user {UserId}, transaction {TransactionId}", userId, tid);
            return new TransactionServiceResult(false, "An error occurred while deleting the transaction", ErrorType.ServerError);
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
            logger.LogError(ex, "ReadTransaction error for user {UserId}", userId);
            return new TransactionServiceResult<List<TransactionResponse>>(false, "An error occurred while retrieving transactions", errorType: ErrorType.ServerError);
        }
    }

    public TransactionServiceResult<TransactionResponse> GetTransactionById(int userId, int tid)
    {
        try
        {
            var result = transactionRepository.GetTransactionById(userId, tid);
            if (!result.Any())
            {
                return new TransactionServiceResult<TransactionResponse>(false, "Transaction not found", errorType: ErrorType.NotFound);
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
            logger.LogError(ex, "GetTransactionById error for user {UserId}, transaction {TransactionId}", userId, tid);
            return new TransactionServiceResult<TransactionResponse>(false, "An error occurred while retrieving the transaction", errorType: ErrorType.ServerError);
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
            logger.LogError(ex, "GetBalance error for user {UserId}", userId);
            return new TransactionServiceResult<BalanceResponse>(false, "An error occurred while retrieving the balance", errorType: ErrorType.ServerError);
        }
    }
}

public class TransactionServiceResult
{
    public bool Success { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    public TransactionServiceResult(bool success, string message, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        ErrorType = errorType;
    }
}

public class TransactionServiceResult<T>
{
    public bool Success { get; }
    public string Message { get; }
    public T? Data { get; }
    public ErrorType ErrorType { get; }

    public TransactionServiceResult(bool success, string message, T? data = default, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        Data = data;
        ErrorType = errorType;
    }
}