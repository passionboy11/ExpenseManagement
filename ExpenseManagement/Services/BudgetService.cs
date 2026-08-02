using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;

namespace ExpenseManagement.Services;

public interface IBudgetService
{
    BudgetServiceResult CreateBudget(int userId, CreateBudget request);
    BudgetServiceResult EditBudget(int userId, int tid, EditBudget request);
    BudgetServiceResult DeleteBudget(int userId, int tid);
    BudgetServiceResult<BudgetResponse> GetBudgetById(int id, int tid);
    BudgetServiceResult<List<BudgetResponse>> ReadBudget(int userId);
}

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository budgetRepository;
    private readonly ILogger<BudgetService> logger;

    public BudgetService(IBudgetRepository budgetRepository, ILogger<BudgetService> logger)
    {
        this.budgetRepository = budgetRepository;
        this.logger = logger;
    }

    public BudgetServiceResult CreateBudget(int userId, CreateBudget request)
    {
        try
        {
            var success = budgetRepository.CreateBudget(request, userId);
            if (!success)
                return new BudgetServiceResult(false, "Failed to create budget. Please try again.", ErrorType.Validation);

            return new BudgetServiceResult(true, "Budget created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CreateBudget error for user {UserId}", userId);
            return new BudgetServiceResult(false, "An error occurred while creating the budget", ErrorType.ServerError);
        }
    }

    public BudgetServiceResult EditBudget(int userId, int tid, EditBudget request)
    {
        try
        {
            var success = budgetRepository.EditBudget(request, tid, userId);
            if (!success)
                return new BudgetServiceResult(false, "Budget not found or update failed", ErrorType.NotFound);

            return new BudgetServiceResult(true, "Budget updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EditBudget error for user {UserId}, budget {BudgetId}", userId, tid);
            return new BudgetServiceResult(false, "An error occurred while updating the budget", ErrorType.ServerError);
        }
    }

    public BudgetServiceResult DeleteBudget(int userId, int tid)
    {
        try
        {
            var success = budgetRepository.DeleteBudget(userId, tid);
            if (!success)
                return new BudgetServiceResult(false, "Budget not found or already deleted", ErrorType.NotFound);

            return new BudgetServiceResult(true, "Budget deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DeleteBudget error for user {UserId}, budget {BudgetId}", userId, tid);
            return new BudgetServiceResult(false, "An error occurred while deleting the budget", ErrorType.ServerError);
        }
    }

    public BudgetServiceResult<List<BudgetResponse>> ReadBudget(int userId)
    {
        try
        {
            var budgets = budgetRepository.ReadBudget(userId);


            var budgetList = budgets.Select(t => new BudgetResponse
            {
                Id = t.Id,
                UserId = t.UserId,
                Email = t.Email,
                Category = t.Category,
                LimitAmount = t.LimitAmount,
                MonthYear = t.MonthYear
            }).ToList();

            // Empty list is valid - user just has no budgets yet (200 OK with empty array)
            var message = budgetList.Any()
                ? "Budgets retrieved successfully"
                : "No budgets found";

            return new BudgetServiceResult<List<BudgetResponse>>(true, message, budgetList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ReadBudget error for user {UserId}", userId);
            return new BudgetServiceResult<List<BudgetResponse>>(false, "An error occurred while retrieving budgets", errorType: ErrorType.ServerError);
        }
    }

    public BudgetServiceResult<BudgetResponse> GetBudgetById(int id, int tid)
    {
        try
        {
            var result = budgetRepository.GetBudgetById(id, tid);
            if (!result.Any())
            {
                return new BudgetServiceResult<BudgetResponse>(false, "Budget not found", errorType: ErrorType.NotFound);
            }

            var budget = result.Select(t => new BudgetResponse
            {
                Id = t.Id,
                UserId = t.UserId,
                Email = t.Email,
                Category = t.Category,
                LimitAmount = t.LimitAmount,
                MonthYear = t.MonthYear
            }).FirstOrDefault();

            return new BudgetServiceResult<BudgetResponse>(true, "Budget retrieved successfully", budget!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetBudgetById error for user {UserId}, budget {BudgetId}", id, tid);
            return new BudgetServiceResult<BudgetResponse>(false, "An error occurred while retrieving the budget", errorType: ErrorType.ServerError);
        }
    }
}

public class BudgetServiceResult
{
    public bool Success { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }

    public BudgetServiceResult(bool success, string message, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        ErrorType = errorType;
    }
}

public class BudgetServiceResult<T>(bool success, string message, T? data = default, ErrorType errorType = ErrorType.Validation)
{
    public bool Success { get; } = success;
    public string Message { get; } = message;
    public T? Data { get; } = data;
    public ErrorType ErrorType { get; } = errorType;
}