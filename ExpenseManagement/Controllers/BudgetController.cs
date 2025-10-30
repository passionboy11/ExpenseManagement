using ExpenseManagement.Infrastructure;
using ExpenseManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BudgetController : ControllerBase
{
    private readonly DataAccess dataAccess;
    public BudgetController(DataAccess dataAccess)
    {
        this.dataAccess = dataAccess;
    }

   
    
    
}