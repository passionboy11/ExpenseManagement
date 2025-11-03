using System.Security.Claims;
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
}