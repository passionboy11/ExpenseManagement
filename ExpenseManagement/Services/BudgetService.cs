//using ExpenseManagement.DTO;
//using ExpenseManagement.Infrastructure;


//namespace ExpenseManagement.Services;

//public interface IBudgetService
//{
//    BudgetServiceResult CreateBudget(int userId,  CreateBudget request);
//    BudgetServiceResult EditBudget(int userId,int tid,  EditBudget request);
//    BudgetServiceResult DeleteBudget(int userId, int tid);
//    BudgetServiceResult <BudgetResponse> GetBudgetById(int id,int tid);
//    BudgetServiceResult <List<BudgetResponse>> ReadBudget(int userId);
//}

//public class BudgetService:IBudgetService
//{
//   private readonly IBudgetRepository budgetRepository;

//   public BudgetService(IBudgetRepository budgetRepository)
//   {
//       this.budgetRepository = budgetRepository;
//   }

//   public BudgetServiceResult CreateBudget(int userId, CreateBudget request)
//   {
//       var success = budgetRepository.CreateBudget(request, userId);
//       if (!success)
//           return new BudgetServiceResult(false, "Error creating budget");

//       return new BudgetServiceResult(true, "Budget created successfully");
//   }

//   public BudgetServiceResult EditBudget(int userId,int tid, EditBudget request)
//   {
//       var success = budgetRepository.EditBudget(request, userId,tid);
//       if(!success)
//           return new BudgetServiceResult(false, "Error editing budget");

//       return new BudgetServiceResult(true, "Budget edited successfully");
//   }

//   public BudgetServiceResult DeleteBudget(int userId, int tid)
//   {
//       var success = budgetRepository.DeleteBudget( userId,tid);
//       if(!success)
//           return new BudgetServiceResult(false, "Error deleting budget");

//       return new BudgetServiceResult(true, "Budget deleted successfully");

//   }

//   public BudgetServiceResult<List<BudgetResponse>> ReadBudget(int userId)
//   {
//       var success = budgetRepository.ReadBudget(userId);
//       if(!success.Any())
//           return new BudgetServiceResult<List<BudgetResponse>>(false,"Error reading budget");
//       var budget = success.Select(t => new BudgetResponse
//       {
//           Id = t.Id,
//           UserId = t.UserId,
//           Email = t.Email,
//           Category = t.Category,
//           LimitAmount = t.LimitAmount,
//           MonthYear = t.MonthYear
//       }).ToList();

//       return new BudgetServiceResult<List<BudgetResponse>>(true,"Budget read successfully", budget);
//   }

//    public BudgetServiceResult<BudgetResponse> GetBudgetById(int id, int tid)
//    {
//       var result = budgetRepository.GetBudgetById(id, tid);
//        if (!result.Any())
//        {
//            return new BudgetServiceResult<BudgetResponse>(false, "Budget not found");
//        }
//        var budget = result.Select(t => new BudgetResponse
//        {
//            Id = t.Id,
//            UserId = t.UserId,
//            Email = t.Email,
//            Category = t.Category,
//            LimitAmount = t.LimitAmount,
//            MonthYear = t.MonthYear
//        }).FirstOrDefault();
//        return new BudgetServiceResult<BudgetResponse>(true, "Budget retrieved successfully", budget!);
//    }
//}

//public class BudgetServiceResult
//{
//    public bool Success { get; }
//    public string Message { get; }

//    public BudgetServiceResult(bool success, string message)
//    {
//        Success = success;
//        Message = message;
//    }
//}

//public class BudgetServiceResult<T>(bool success, string message, T? data = default)
//{
//    public bool Success { get; } = success;
//    public string Message { get; } = message;
//    public T? Data { get; } = data;
//}
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

    public BudgetService(IBudgetRepository budgetRepository)
    {
        this.budgetRepository = budgetRepository;
    }

    public BudgetServiceResult CreateBudget(int userId, CreateBudget request)
    {
        try
        {
            var success = budgetRepository.CreateBudget(request, userId);
            if (!success)
                return new BudgetServiceResult(false, "Failed to create budget. Please try again.");

            return new BudgetServiceResult(true, "Budget created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CreateBudget error: {ex.Message}");
            return new BudgetServiceResult(false, "An error occurred while creating the budget");
        }
    }

    public BudgetServiceResult EditBudget(int userId, int tid, EditBudget request)
    {
        try
        {
            var success = budgetRepository.EditBudget(request, tid, userId);
            if (!success)
                return new BudgetServiceResult(false, "Budget not found or update failed");

            return new BudgetServiceResult(true, "Budget updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EditBudget error: {ex.Message}");
            return new BudgetServiceResult(false, "An error occurred while updating the budget");
        }
    }

    public BudgetServiceResult DeleteBudget(int userId, int tid)
    {
        try
        {
            var success = budgetRepository.DeleteBudget(userId, tid);
            if (!success)
                return new BudgetServiceResult(false, "Budget not found or already deleted");

            return new BudgetServiceResult(true, "Budget deleted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeleteBudget error: {ex.Message}");
            return new BudgetServiceResult(false, "An error occurred while deleting the budget");
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

            // Return success even if empty - different message for clarity
            var message = budgetList.Any()
                ? "Budgets retrieved successfully"
                : "No budgets found";

            return new BudgetServiceResult<List<BudgetResponse>>(true, message, budgetList);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReadBudget error: {ex.Message}");
            return new BudgetServiceResult<List<BudgetResponse>>(false, "An error occurred while retrieving budgets");
        }
    }

    public BudgetServiceResult<BudgetResponse> GetBudgetById(int id, int tid)
    {
        try
        {
            var result = budgetRepository.GetBudgetById(id, tid);
            if (!result.Any())
            {
                return new BudgetServiceResult<BudgetResponse>(false, "Budget not found");
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
            Console.WriteLine($"GetBudgetById error: {ex.Message}");
            return new BudgetServiceResult<BudgetResponse>(false, "An error occurred while retrieving the budget");
        }
    }
}

public class BudgetServiceResult
{
    public bool Success { get; }
    public string Message { get; }

    public BudgetServiceResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

public class BudgetServiceResult<T>(bool success, string message, T? data = default)
{
    public bool Success { get; } = success;
    public string Message { get; } = message;
    public T? Data { get; } = data;
}