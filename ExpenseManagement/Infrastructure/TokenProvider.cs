using System.Security.Claims;
using System.Text;
using ExpenseManagement.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseManagement.Infrastructure;

public class TokenProvider
{
    //It stores an IConfiguration object, which is used in ASP.NET Core to access values from:
    // appsettings.json
    // Environment variables
    // User secrets
    // Other configuration providers
    private readonly IConfiguration configuration;
//      This is the constructor of the TokenProvider class.
//      ASP.NET Core uses dependency injection (DI) to pass in the IConfiguration automatically.
//      When TokenProvider is created, ASP.NET Core will give it the app’s IConfiguration object.

    public TokenProvider(IConfiguration configuration)
    {
        //Assigns the injected configuration to the class field
        // After this, any method in TokenProvider can use this.configuration to access settings.
        this.configuration = configuration;
    }


    //Use Random opaque string for refresh token generation and hash and store it securely in a database.

    public Token GenerateToken(UserAccount userAccount)
    {
        var accessToken = GenerateAccessToken(userAccount);
        var refreshToken = GenerateRefreshToken();
        return new Token
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    private RefreshToken GenerateRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            Expires = DateTime.Now.AddMonths(1),
            CreatedDate = DateTime.Now,
            Enabled = true,
        };
        return refreshToken;
    }
    private string GenerateAccessToken(UserAccount userAccount)
    {
        string secretKey = configuration["JWT:SecretKey"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
                new Claim(ClaimTypes.Name, userAccount.Email),
                new Claim(ClaimTypes.Role, userAccount.Role)
            ]), 
            
            Expires = DateTime.Now.AddSeconds(600),
            SigningCredentials = credentials,
            Issuer = configuration["JWT:Issuer"],
            Audience = configuration["JWT:Audience"],
            
        };
        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
} 
public class Token
{
    public string AccessToken {get; set; }
    public RefreshToken RefreshToken {get; set; }
}