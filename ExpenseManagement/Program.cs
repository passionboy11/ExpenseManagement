using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ExpenseManagement.ExceptionHandler;
using ExpenseManagement.Extension;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Services;

var builder = WebApplication.CreateBuilder(args);
var frontendOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173";
// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddScoped<AdminService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173")
            .WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});
// Register global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
// Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerAuth(); // Make sure this configures JWT support in Swagger

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Service registrations
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IReminderRepository, ReminderRepository>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddHttpContextAccessor(); // this is required
builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<TokenProvider>();

var app = builder.Build();

app.UseCors("AllowReactApp");
// Middleware pipeline
app.UseExceptionHandler(opt => { });
// Use Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// **Order matters**: Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Run the application
app.Run();