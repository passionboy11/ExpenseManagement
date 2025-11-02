using System.Security.Claims;
using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;
// system claim esma garne ani tanne 
// usercontext class 
// iusercontext interface 
// get user email role bata sidhai name dios
[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
  private readonly ITransactionService transactionService;
  public TransactionController(ITransactionService transactionService)
   {
    this.transactionService = transactionService;
   }
 
  [Authorize]
  [HttpPost("createtransaction")]
  public IActionResult CreateTransaction([FromBody] CreateTransactionRequest request)
   {
     var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
    
     var result = transactionService.CreateTransaction(request,emailClaim);
    
     if (!result.Success)
     {
      return BadRequest(new{Message= result.Message});
     }
     return Ok(new{Message="Transaction created successfully"});
   }
   
    [Authorize]
    [HttpPut("edittransaction")]
    public IActionResult EditTransaction([FromBody] EditTransactionRequest request)
    {
     var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
   
     var result = transactionService.EditTransaction(request, emailClaim);
     if (!result.Success)
     {
      return BadRequest(new{Message= result.Message});
     }
     return Ok(new{Message="Edited successfully"});
    }
   
    [Authorize]
    [HttpDelete("deletetransaction")]
    public IActionResult DeleteTransaction([FromBody] DeleteTransactionRequest request)
    {
      var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
     
      var result = transactionService.DeleteTransaction(request, emailClaim);
      
      if (!result.Success)
      {
       return BadRequest(new{Message= result.Message});
      }
      return Ok(new{Message="Edited successfully"});
    }
   
    [Authorize]
    [HttpGet("gettransaction")]
    public IActionResult ReadTransaction()
    {
     var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
     
     var result =  transactionService.ReadTransaction(emailClaim);
     
     if(!result.Success)
      {
      return BadRequest(new{Message= result.Message});
      }

     return Ok(new { Message = result.Message, Data = result.Data });
    }
   
   
}
 