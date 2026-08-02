using ExpenseManagement.ExceptionHandler;
using ExpenseManagement.Extension;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});


// Disable appsettings.json file watcher for Render Linux containers
builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: false
    )
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: false
    )
    .AddEnvironmentVariables();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerAuth();


builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization();


// Database
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();


// Repositories
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
        limiterOptions.PermitLimit = 100;

        limiterOptions.Window = TimeSpan.FromMinutes(1);

        limiterOptions.QueueProcessingOrder =
            QueueProcessingOrder.OldestFirst;

        limiterOptions.QueueLimit = 0;
    });
});


var app = builder.Build();


app.UseCors("AllowReactApp");

app.UseRateLimiter();

app.UseExceptionHandler(opt => { });


// Swagger
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();


app.UseAuthentication();

app.UseAuthorization();


app.MapControllers()
    .RequireRateLimiting("fixed");


app.Run();