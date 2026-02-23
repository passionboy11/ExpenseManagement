using ExpenseManagement.Infrastructure;
using ExpenseManagement.DTO;
namespace ExpenseManagement.Services;

public interface IAuthService
{
    
    AuthServiceResult RegisterUser(string email, string password, string role);
    AuthServiceResult<AuthResponse> Login(string email, string password);
    AuthServiceResult<AuthResponse> RefreshToken(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly TokenProvider tokenProvider;
    private readonly IAuthRepository authRepository;

    public AuthService(IAuthRepository authRepository, TokenProvider tokenProvider)
    {
        this.tokenProvider = tokenProvider;
        this.authRepository = authRepository;
    }

    public AuthServiceResult RegisterUser(string email, string password, string role)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        string Role = string.IsNullOrWhiteSpace(role) ? "User" : role;
        var result = authRepository.RegisterUser(email, hashedPassword, Role);
        if (!result)
            return new AuthServiceResult(false, "Registration failed.");

        return new AuthServiceResult(true, "Registration successful.");
    }

    public AuthServiceResult<AuthResponse> Login(string email, string password)
    {
        var user = authRepository.FindUserByEmail(email);
        if (user == null)
            return new AuthServiceResult<AuthResponse>(false,"User is not found");

        var verifyPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
        if (!verifyPassword)
            return new AuthServiceResult<AuthResponse>(false,"Wrong Password");

       
        var token = tokenProvider.GenerateToken(user);
      
        authRepository.DisableUserTokenByEmail(email);
        authRepository.InsertRefreshtoken(token.RefreshToken, email);
        
        var authResponse = new AuthResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken.Token,
            Expires = token.RefreshToken.Expires
        };
        return new AuthServiceResult<AuthResponse>(true, "Login successful.", authResponse);
    }
    public AuthServiceResult<AuthResponse> RefreshToken(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return new AuthServiceResult<AuthResponse>(false,"RefreshToken is empty");
        }
        var isValid = authRepository.IsRefreshTokenValid(refreshToken);
        if(!isValid)
            return new AuthServiceResult<AuthResponse>(false,"RefreshToken is invalid");
        
        var currentUser = authRepository.FindUserByToken(refreshToken);
        if (currentUser == null)
            return new AuthServiceResult<AuthResponse>(false,"User is not found");
        
        var token = tokenProvider.GenerateToken(currentUser);
        authRepository.DisableUserToken(refreshToken);
        authRepository.InsertRefreshtoken(token.RefreshToken, currentUser.Email);
        
        var authResponse = new AuthResponse
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken.Token,
            Expires = token.RefreshToken.Expires
        };
        return new AuthServiceResult<AuthResponse>(true, "Token refreshed successfully.", authResponse);
    }

   
}

public class AuthServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public AuthServiceResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

public class AuthServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public AuthServiceResult(bool success, string message,T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
