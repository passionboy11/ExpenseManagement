using System.Security.Claims;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.DTO;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Services;

namespace ExpenseManagement.Controllers;

[Route("[controller]")]
[ApiController]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService budgetService;
    private readonly IUserContext userContext;
    public BudgetController(IBudgetService budgetService, IUserContext userContext)
    {
        this.budgetService = budgetService;
        this.userContext  = userContext;
    }

    [Authorize]
    [HttpPost("createbudget")]
    public IActionResult CreateBudget([FromBody] Budget request)
    {
        int id = userContext.GetUserId();
        
        var result = budgetService.CreateBudget(id,request);

        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message="Budget created successfully"});

    }

    [Authorize]
    [HttpPut("editbudget/{tid}")]
    public IActionResult EditBudget([FromBody] EditBudget request, int tid)
    {
        int id = userContext.GetUserId();
        var result = budgetService.EditBudget(id,tid,request);
        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message="Budget edited successfully"});

    }

    [Authorize]
    [HttpDelete("deletebudget/{tid}")]
    public IActionResult DeleteBudget([FromBody] DeleteBudget request, int tid)
    {
        int id = userContext.GetUserId();
        
        var result = budgetService.DeleteBudget(id,tid,request);
        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message="Budget deleted successfully"});
    }

    [Authorize]
    [HttpGet("viewbudget")]
    public IActionResult GetBudget()
    {
        int id = userContext.GetUserId();
        Console.WriteLine(id);
        var result = budgetService.ReadBudget(id);
        if(!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }
    [Authorize]
    [HttpGet("getbudgetbyid/{tid}")]
    public IActionResult GetBudgetById(int tid)
    {
        int id = userContext.GetUserId();
        
        var result = budgetService.GetBudgetById(id,tid);
        if(!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new { Message = result.Message, Data = result.Data });
    }

}