using ExpenseManagement.ExceptionHandler;
using ExpenseManagement.Extension;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddScoped<AdminService>();

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerAuth();

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();


// Repositories
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();


// Services
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();


// Helpers
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<TokenProvider>();


// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        // Maximum requests allowed
        limiterOptions.PermitLimit = 100;

        // Time window
        limiterOptions.Window = TimeSpan.FromMinutes(1);

        // Requests waiting in queue
        limiterOptions.QueueLimit = 0;

        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});


var app = builder.Build();


// CORS must come before authentication/authorization
app.UseCors("AllowReactApp");


// Rate limiter middleware
app.UseRateLimiter();


// Exception handling
app.UseExceptionHandler(opt => { });


// Swagger
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();


// Authentication must come before Authorization
app.UseAuthentication();

app.UseAuthorization();


// Apply rate limit to all controllers
app.MapControllers()
    .RequireRateLimiting("fixed");


app.Run();