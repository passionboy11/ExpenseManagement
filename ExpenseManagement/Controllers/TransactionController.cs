using ExpenseManagement.DTO;
using ExpenseManagement.Services;
using ExpenseManagement.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService transactionService;
    private readonly IUserContext userContext;
    public TransactionController(ITransactionService transactionService, IUserContext userContext)
    {
        this.transactionService = transactionService;
        this.userContext = userContext;
    }


    [Authorize]
    [HttpPost("transactions")]
    public IActionResult CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        int id = userContext.GetUserId();

        var result = transactionService.CreateTransaction(request, id);

        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }
        return Ok(new
        {
            Message = result.Message
        });
    }

    [Authorize]
    [HttpPut("transactions/{tid}")]
    public IActionResult EditTransaction([FromBody] EditTransactionRequest request, int tid)
    {
        int id = userContext.GetUserId();

        var result = transactionService.EditTransaction(request, id, tid);
        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }
        return Ok(new { Message = result.Message });
    }

    [Authorize]
    [HttpDelete("transactions/{tid}")]
    public IActionResult DeleteTransaction([FromRoute] int tid)
    {
        int id = userContext.GetUserId();

        var result = transactionService.DeleteTransaction(id, tid);

        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }
        return Ok(new { Message = result.Message });
    }

    [Authorize]
    [HttpGet("transactions")]
    public IActionResult ReadTransaction()
    {
        int id = userContext.GetUserId();

        var result = transactionService.ReadTransaction(id);

        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }

    [Authorize]
    [HttpGet("transactions/{tid}")]
    public IActionResult GetTransactionById([FromRoute] int tid)
    {
        int id = userContext.GetUserId();
        var result = transactionService.GetTransactionById(id, tid);
        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }

        return Ok(new { Message = result.Message, Data = result.Data });
    }

    [Authorize]
    [HttpGet("balance")]
    public IActionResult GetBalance()
    {
        int id = userContext.GetUserId();
        var result = transactionService.GetBalance(id);
        if (!result.Success)
        {
            return this.ToErrorResult(result.ErrorType, result.Message);
        }
        return Ok(new { Message = result.Message, Data = result.Data });
    }
}