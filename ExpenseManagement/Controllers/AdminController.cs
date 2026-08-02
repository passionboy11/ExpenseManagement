using ExpenseManagement.Infrastructure;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;


[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService adminService;

    public AdminController(IAdminService adminService)
    {
        this.adminService = adminService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("transactions")]
    public IActionResult ReadAllTransactions()
    {
        var result = adminService.ReadAllTransactions();
        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("budgets")]
    public IActionResult ReadAllBudgets()
    {
        var result = adminService.ReadAllBudgets();
        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public IActionResult ViewAllUsers()
    {
        var result = adminService.ViewAllUsers();
        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("reminders")]
    public IActionResult ReadAllReminders()
    {
        var result = adminService.ReadAllReminders();
        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }

}