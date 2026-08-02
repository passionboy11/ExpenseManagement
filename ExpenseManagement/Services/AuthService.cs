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
    private readonly ILogger<AuthService> logger;

    public AuthService(IAuthRepository authRepository, TokenProvider tokenProvider, ILogger<AuthService> logger)
    {
        this.tokenProvider = tokenProvider;
        this.authRepository = authRepository;
        this.logger = logger;
    }

    public AuthServiceResult RegisterUser(string email, string password, string role)
    {
        try
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            string Role = string.IsNullOrWhiteSpace(role) ? "User" : role;
            var result = authRepository.RegisterUser(email, hashedPassword, Role);
            if (!result)
                return new AuthServiceResult(false, "An account with this email already exists.", ErrorType.Conflict);

            return new AuthServiceResult(true, "Registration successful.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RegisterUser error for {Email}", email);
            return new AuthServiceResult(false, "An error occurred while registering.", ErrorType.ServerError);
        }
    }

    public AuthServiceResult<AuthResponse> Login(string email, string password)
    {
        try
        {
            var user = authRepository.FindUserByEmail(email);
            if (user == null)
                return new AuthServiceResult<AuthResponse>(false, "Invalid email or password", errorType: ErrorType.Unauthorized);

            var verifyPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!verifyPassword)
                return new AuthServiceResult<AuthResponse>(false, "Invalid email or password", errorType: ErrorType.Unauthorized);


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
        catch (Exception ex)
        {
            logger.LogError(ex, "Login error for {Email}", email);
            return new AuthServiceResult<AuthResponse>(false, "An error occurred while logging in.", errorType: ErrorType.ServerError);
        }
    }
    public AuthServiceResult<AuthResponse> RefreshToken(string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return new AuthServiceResult<AuthResponse>(false, "RefreshToken is empty", errorType: ErrorType.Unauthorized);
            }
            var isValid = authRepository.IsRefreshTokenValid(refreshToken);
            if (!isValid)
                return new AuthServiceResult<AuthResponse>(false, "RefreshToken is invalid", errorType: ErrorType.Unauthorized);

            var currentUser = authRepository.FindUserByToken(refreshToken);
            if (currentUser == null)
                return new AuthServiceResult<AuthResponse>(false, "User is not found", errorType: ErrorType.Unauthorized);

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
        catch (Exception ex)
        {
            logger.LogError(ex, "RefreshToken error");
            return new AuthServiceResult<AuthResponse>(false, "An error occurred while refreshing the token.", errorType: ErrorType.ServerError);
        }
    }


}

public class AuthServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ErrorType ErrorType { get; set; }
    public AuthServiceResult(bool success, string message, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        ErrorType = errorType;
    }
}

public class AuthServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public ErrorType ErrorType { get; set; }

    public AuthServiceResult(bool success, string message, T? data = default, ErrorType errorType = ErrorType.Validation)
    {
        Success = success;
        Message = message;
        Data = data;
        ErrorType = errorType;
    }
}