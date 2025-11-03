using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;
[Route("[controller]")]
[ApiController]
public class ResourceController : ControllerBase
{
    [Authorize]
    [HttpGet]
    [Route("verify")]
    public IActionResult Verify()
    {
        return Ok("You are authorized.");
    }
}