//using ExpenseManagement.Infrastructure;

//namespace ExpenseManagement.Services;

//public class AuthService
//{
//    private readonly TokenProvider tokenProvider;
//    private readonly IAuthRepository authRepository;

//    public AuthService(IAuthRepository authRepository, TokenProvider tokenProvider)
//    {
//        this.tokenProvider = tokenProvider;
//        this.authRepository = authRepository;
//    }
//   public AuthServiceResult RegisterUser(string email , string password, string role)
//    {
//        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
//        string role = string.IsNullOrWhiteSpace(role) ? "User" : role;
//        var result = authRepository.RegisterUser(email, hashedPassword, role);
//        if(!result) 
//            return new AuthServiceResult(false, "Registration failed.");

//    }
//public class AuthServiceResult
//{
//    public bool Success { get; set; }
//    public string Message { get; set; }
//    public AuthServiceResult(bool success, string message)
//    {
//        Success = success;
//        Message = message;
//    }
//}

//public class AuthServiceResult<T>
//{
//    public bool Success { get; set; }
//    public string Message { get; set; }
//    public T? Data { get; set; }

//    public AuthServiceResult(bool success, string message,T? data = null)
//    {
//        Success = success;
//        Message = message;
//        Data = data;
//    }
//}
