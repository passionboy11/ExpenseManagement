using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ExpenseManagement.Extension;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerAuth();

builder.Services.AddAuthorization();
// registers authentication services in the app
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    // Forces the app to accept token only over HTTPS
    opt.RequireHttpsMetadata = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        // Defines the secret key used to validate the token's signature
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        // Ensures the token was issued by trusted issuer
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        // Ensures the token was meant for this application
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // By default ASP.NET allows 5 minute clock skew to handle server time difference
        // Setting it to zero means the token expires exactly at its expiration time
        ClockSkew = TimeSpan.Zero,
    };
});

builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<TokenProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();