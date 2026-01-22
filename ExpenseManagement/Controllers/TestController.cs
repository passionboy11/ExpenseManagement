using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[ApiController]
[Route("api/[controller]")] // important
public class TestController : ControllerBase
{
    private readonly AdminService _adminService;

    public TestController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("viewdata")] // endpoint: GET api/test/viewdata
    public IActionResult ReadAllTransactions()
    {
        var result = _adminService.ReadAllTransactions();
        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }
}