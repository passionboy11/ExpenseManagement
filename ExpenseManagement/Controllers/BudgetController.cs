using ExpenseManagement.DTO;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[Route("api/[controller]")]
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
    [HttpPost("budgets")]
    public IActionResult CreateBudget([FromBody] CreateBudget request)
    {
        int id = userContext.GetUserId();
        
        var result = budgetService.CreateBudget(id,request);

        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message= result.Message});

    }

    [Authorize]
    [HttpPut("budgets/{tid}")]
    public IActionResult EditBudget(EditBudget request, [FromRoute] int tid)
    {
        int id = userContext.GetUserId();
        var result = budgetService.EditBudget(id,tid,request);
        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message= result.Message });

    }

    [Authorize]
    [HttpDelete("budgets/{tid}")]
    public IActionResult DeleteBudget([FromRoute] int tid)
    {
        int id = userContext.GetUserId();
        
        var result = budgetService.DeleteBudget(id,tid);
        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message= result.Message });
    }

    [Authorize]
    [HttpGet("budgets")]
    public IActionResult GetBudget()
    {
        int id = userContext.GetUserId();
        var result = budgetService.ReadBudget(id);
        if(!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }
    [Authorize]
    [HttpGet("budgets/{tid}")]
    public IActionResult GetBudgetById([FromRoute] int tid)
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