using System.Security.Claims;
using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionController : ControllerBase
{
  private readonly DataAccess dataAccess;
  public TransactionController(DataAccess dataAccess)
   {
    this.dataAccess = dataAccess;
   }
 
  [Authorize]
  [HttpPost("createtransaction")]
  public IActionResult CreateTransaction([FromBody] CreateTransactionRequest request)
   {
     if (request == null)
      return BadRequest(new { Message = "Invalid request" });

     var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
     if (string.IsNullOrEmpty(emailClaim))
      return Unauthorized();
     
     var userId = dataAccess.FindUserIdByEmail(emailClaim);
     if (userId == null)
      return Unauthorized();
     
     // Call synchronous data access method
     var success = dataAccess.CreateTransaction(request,userId);
   
     if (success)
      return Ok(new { Message = "Transaction created successfully" });

     return BadRequest(new { Message = "Failed to create transaction" });
   }

   [Authorize]
   [HttpPut("edittransaction")]
   public IActionResult EditTransaction([FromBody] EditTransactionRequest request)
   {
    if (request == null)
     return BadRequest(new { Message = "Invalid request" });

    var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(emailClaim))
     return Unauthorized();

    var userId = dataAccess.FindUserIdByEmail(emailClaim);
    if (userId == null)
     return Unauthorized();

    var success = dataAccess.EditTransaction(request, userId);
    if (success)
     return Ok(new { Message = "Transaction edited successfully" });

    return BadRequest(new { Message = "Failed to edit transaction" });
   }

   [Authorize]
   [HttpDelete("deletetransaction")]
   public IActionResult DeleteTransaction([FromBody] DeleteTransactionRequest request)
   {
    if (request == null)
        return BadRequest(new { Message = "Invalid request" });
    
    var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(emailClaim))
      return Unauthorized();
    
    var userId = dataAccess.FindUserIdByEmail(emailClaim);
    if (userId == null)
      return Unauthorized();
    
    var success = dataAccess.DeleteTransaction(request, userId);
    if (success)
     return Ok(new { Message = "Transaction deleted successfully" });
    
    return BadRequest(new { Message = "Failed to delete transaction" });
   }

   [Authorize]
   [HttpGet("gettransaction")]
   public IActionResult ReadTransaction()
   {
    var emailClaim = User.FindFirst(ClaimTypes.Name)?.Value;
    if (string.IsNullOrEmpty(emailClaim))
     return Unauthorized();

    // Get userId from email
    var userId = dataAccess.FindUserIdByEmail(emailClaim);
    if (userId == null)
     return Unauthorized();

    // Get transactions
    var transactions = dataAccess.ReadTransaction(userId);
    if (transactions == null)
     return NotFound("No Transactions found");
    return Ok(transactions);
   }
}
 