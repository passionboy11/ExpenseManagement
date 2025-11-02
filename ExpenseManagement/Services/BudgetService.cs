using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ExpenseManagement.Services;

public interface IBudgetService
{
    BudgetServiceResult CreateBudget(string email,  Budget request);
    BudgetServiceResult EditBudget(string email,  EditBudget request);
    BudgetServiceResult DeleteBudget(string email,  DeleteBudget request);
    BudgetServiceResult <List<BudgetResponse>> ReadBudget(string email);
}

public class BudgetService:IBudgetService
{
   private readonly DataAccess dataAccess;

   public BudgetService(DataAccess dataAccess)
   {
       this.dataAccess = dataAccess;
   }

   public BudgetServiceResult CreateBudget(string email, Budget request)
   {
       var userId = dataAccess.FindUserIdByEmail(email);
       if (userId is 0)
           return new BudgetServiceResult(false, "User not found");
       
       var success = dataAccess.CreateBudget(request, userId);
       if (!success)
           return new BudgetServiceResult(false, "Error creating budget");
       
       return new BudgetServiceResult(true, "Budget created successfully");
   }

   public BudgetServiceResult EditBudget(string email, EditBudget request)
   {
       var userId = dataAccess.FindUserIdByEmail(email);
       
       var success = dataAccess.EditBudget(request, userId);
       if(!success)
           return new BudgetServiceResult(false, "Error editing budget");
       
       return new BudgetServiceResult(true, "Budget edited successfully");
   }

   public BudgetServiceResult DeleteBudget(string email, DeleteBudget request)
   {
       var userId = dataAccess.FindUserIdByEmail(email);
       
       var success = dataAccess.DeleteBudget(request, userId);
       if(!success)
           return new BudgetServiceResult(false, "Error deleting budget");
       
       return new BudgetServiceResult(true, "Budget deleted successfully");
       
   }

   public BudgetServiceResult<List<BudgetResponse>> ReadBudget(string email)
   {
       var userId = dataAccess.FindUserIdByEmail(email);
       
       var success = dataAccess.ReadBudget(userId);
       if(!success.Any())
           return new BudgetServiceResult<List<BudgetResponse>>(false,"Error reading budget");
       var budget = success.Select(t => new BudgetResponse
       {
           Category = t.Category,
           LimitAmount = t.LimitAmount,
           MonthYear = t.MonthYear
       }).ToList();
       
       return new BudgetServiceResult<List<BudgetResponse>>(true,"Budget read successfully", budget);
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

public class BudgetServiceResult<T>
{
    public bool Success { get; }
    public string Message { get; }
    public T? Data { get; }
    
    public  BudgetServiceResult(bool success, string message, T? data=default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
