using System.Security.Claims;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.DTO;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Services;

namespace ExpenseManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService budgetService;
    public BudgetController(IBudgetService budgetService)
    {
        this.budgetService = budgetService;
    }

    [Authorize]
    [HttpPost("createbudget")]
    public IActionResult CreateBudget([FromBody] Budget request)
    {
        var email = User.FindFirst(ClaimTypes.Name) ?.Value;
        
        var result = budgetService.CreateBudget(email,request);

        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message="Budget created successfully"});

    }

    [Authorize]
    [HttpPut("editbudget")]
    public IActionResult EditBudget([FromBody] EditBudget request)
    {
        var email = User.FindFirst(ClaimTypes.Name) ?.Value;
        var result = budgetService.EditBudget(email,request);
        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message="Budget edited successfully"});

    }

    [Authorize]
    [HttpDelete("deletebudget")]
    public IActionResult DeleteBudget([FromBody] DeleteBudget request)
    {
        var email = User.FindFirst(ClaimTypes.Name) ?.Value;
        var result = budgetService.DeleteBudget(email,request);
        if (!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }
        return Ok(new{Message="Budget deleted successfully"});
    }

    [Authorize]
    [HttpGet("getbudget")]
    public IActionResult GetBudget()
    {
        var emailClaim =  User.FindFirst(ClaimTypes.Name) ?.Value;
        var result = budgetService.ReadBudget(emailClaim);
        if(!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }
    
}