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
[Route("[controller]")]
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
 
 
  [Authorize(Roles = "User")]
  [HttpPost("createtransaction")]
  public IActionResult CreateTransaction([FromBody] CreateTransactionRequest request)
   {
     int id = userContext.GetUserId();

     var result = transactionService.CreateTransaction(request,id);
    
     if (!result.Success)
     {
      return BadRequest(new{Message= result.Message});
     }
     return Ok(new
     {
      Message = result.Message,
      Alert = result.Alert
     });
   }
   
    [Authorize]
    [HttpPut("edittransaction")]
    public IActionResult EditTransaction([FromBody] EditTransactionRequest request)
    {
     int id = userContext.GetUserId();
   
     var result = transactionService.EditTransaction(request, id);
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
      int id = userContext.GetUserId();
     
      var result = transactionService.DeleteTransaction(request, id);
      
      if (!result.Success)
      {
       return BadRequest(new{Message= result.Message});
      }
      return Ok(new{Message="Edited successfully"});
    }
   
    [Authorize]
    [HttpGet("readtransaction")]
    public IActionResult ReadTransaction()
    {
     int id = userContext.GetUserId();
     
     var result =  transactionService.ReadTransaction(id);
     
     if(!result.Success)
      {
      return BadRequest(new{Message= result.Message});
      }

     return Ok(new { Message = result.Message, Data = result.Data });
    }
    
    [Authorize]
    [HttpGet("getbalance")]
    public IActionResult GetBalance()
    {
      int id = userContext.GetUserId();
      var result = transactionService.GetBalance(id);
      if (!result.Success)
       {
        return BadRequest(new{Message= result.Message});
       }
      return Ok(new { Message = result.Message, Data = result.Data });
     }
}
 