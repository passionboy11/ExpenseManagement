using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;


[Route("[controller]")]
[ApiController]
public class AdminController:ControllerBase
{
    private readonly IAdminService adminService;

    public AdminController(IAdminService adminService)
    {
        this.adminService = adminService;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("readalltransactions")]
    public IActionResult ReadAllTransactions()
    {
        var result =  adminService.ReadAllTransactions();
        if(!result.Success)
        {
            return BadRequest(new{Message= result.Message});
        }

        return Ok(new { Message = result.Message,Data=result.Data });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("readallbudgets")]
    public IActionResult ReadAllBudgets()
    {
        var result = adminService.ReadAllBudgets();
        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }
    

    [Authorize(Roles = "Admin")]
    [HttpGet("viewallusers")]
    public IActionResult ViewAllUsers()
    {
        var result = adminService.ViewAllUsers();
        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("readallreminders")]
    public IActionResult ReadAllReminders()
    {
        var result = adminService.ReadAllReminders();
        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }

}