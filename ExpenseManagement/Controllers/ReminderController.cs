using ExpenseManagement.DTO.ReminderDTO;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[ApiController]
[Route("[controller]")]
public class ReminderController : ControllerBase
{
    
private readonly IReminderService reminderService;
private readonly IUserContext userContext;
    public ReminderController (IReminderService reminderService, IUserContext userContext)
    {
        this.reminderService = reminderService;
        this.userContext = userContext;
    }

    [Authorize]
    [HttpPost("createreminder")]
    public IActionResult CreateReminder(CreateReminderRequest request)
    {
        int id = userContext.GetUserId();
        var result = reminderService.CreateReminder(request, id);
        if (!result.Success)
        {
            return BadRequest(new{Message = result.Message});
        }
        return Ok(new
        {
            Message = result.Message,
        });
    }

    [Authorize]
    [HttpPost("editreminder/{rid}")]
    public IActionResult EditReminder(EditReminderRequest request, int rid)
    {
        int id = userContext.GetUserId();
        var result = reminderService.EditReminder(request, id, rid);
        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new
        {
            Message = result.Message,
        });

    }
    [Authorize]
    [HttpDelete("deletereminder/{rid}")]
    public IActionResult DeleteReminder(int rid)
    {
        int id = userContext.GetUserId();
        var result = reminderService.DeleteReminder(id, rid);
        if (!result.Success)
        {
            return BadRequest(new { Message=result.Message });
        }
        return Ok(new
        {
            Message = result.Message
        });
    }

    [Authorize]
    [HttpGet("getreminder/{rid}")]
    public IActionResult GetReminderById(int rid)
    {
        int id = userContext.GetUserId();
        var result = reminderService.GetReminderById(id, rid);
        if (!result.Success)
            return BadRequest(new { Message = result.Message });

        return Ok(new
        {
            Message = result.Message,Data = result.Data
        });
    }

    [Authorize]
    [HttpGet("getreminders")]
    public IActionResult GetReminders()
    {
        int id =  userContext.GetUserId();
        var result = reminderService.GetReminder(id);
        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }
}