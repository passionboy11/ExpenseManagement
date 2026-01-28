using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ExpenseManagement.Services;

public interface IBudgetService
{
    BudgetServiceResult CreateBudget(int userId,  Budget request);
    BudgetServiceResult EditBudget(int userId,int tid,  EditBudget request);
    BudgetServiceResult DeleteBudget(int userId, int tid,  DeleteBudget request);
    BudgetServiceResult <BudgetResponse> GetBudgetById(int id,int tid);
    BudgetServiceResult <List<BudgetResponse>> ReadBudget(int userId);
}

public class BudgetService:IBudgetService
{
   private readonly DataAccess dataAccess;

   public BudgetService(DataAccess dataAccess)
   {
       this.dataAccess = dataAccess;
   }

   public BudgetServiceResult CreateBudget(int userId, Budget request)
   {
       var success = dataAccess.CreateBudget(request, userId);
       if (!success)
           return new BudgetServiceResult(false, "Error creating budget");
       
       return new BudgetServiceResult(true, "Budget created successfully");
   }

   public BudgetServiceResult EditBudget(int userId,int tid, EditBudget request)
   {
       var success = dataAccess.EditBudget(request, userId,tid);
       if(!success)
           return new BudgetServiceResult(false, "Error editing budget");
       
       return new BudgetServiceResult(true, "Budget edited successfully");
   }

   public BudgetServiceResult DeleteBudget(int userId, int tid, DeleteBudget request)
   {
       var success = dataAccess.DeleteBudget(request, userId,tid);
       if(!success)
           return new BudgetServiceResult(false, "Error deleting budget");
       
       return new BudgetServiceResult(true, "Budget deleted successfully");
       
   }

   public BudgetServiceResult<List<BudgetResponse>> ReadBudget(int userId)
   {
       var success = dataAccess.ReadBudget(userId);
       if(!success.Any())
           return new BudgetServiceResult<List<BudgetResponse>>(false,"Error reading budget");
       var budget = success.Select(t => new BudgetResponse
       {
           Id = t.Id,
           UserId = t.UserId,
           Email = t.Email,
           Category = t.Category,
           LimitAmount = t.LimitAmount,
           MonthYear = t.MonthYear
       }).ToList();
       
       return new BudgetServiceResult<List<BudgetResponse>>(true,"Budget read successfully", budget);
   }

    public BudgetServiceResult<BudgetResponse> GetBudgetById(int id, int tid)
    {
       var result = dataAccess.GetBudgetById(id, tid);
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
