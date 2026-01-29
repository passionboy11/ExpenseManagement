using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenProvider tokenProvider;
        private readonly  IAuthRepository authRepository;

        // Controller depends on DataAccess for db operation
        // This is injected via Dependency Injection (DI)
        public AuthController(IAuthRepository authRepository, TokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;
            this.authRepository = authRepository;
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;
            var result = authRepository.RegisterUser(request.Email, hashedPassword,role);
            if (result)
            {

                return Ok(new {message = "User Registered Successfully"});

            }
            else
            {
                return BadRequest("Failed to register user");
            }

        }

        [HttpPost("Login")]
        public ActionResult<AuthResponse> Login(AuthRequest request)
        {
            AuthResponse response = new AuthResponse();

            var user = authRepository.FindUserByEmail(request.Email);
            if (user == null)
                return BadRequest("User is not found");

            var verifyPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!verifyPassword)
                return BadRequest("Wrong Password");

            // Generate Access token
            var token = tokenProvider.GenerateToken(user);
            response.AccessToken = token.AccessToken;


            authRepository.DisableUserTokenByEmail(request.Email);
            authRepository.InsertRefreshtoken(token.RefreshToken, request.Email);
            
            Response.Cookies.Append("refreshToken", token.RefreshToken.Token, new CookieOptions
            {
                HttpOnly =true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = token.RefreshToken.Expires,
                Path = "/"
            });

            return Ok(new AuthResponse
            {
                AccessToken = token.AccessToken,
            });
        }

        [HttpPost("refresh-token")]
        public ActionResult<AuthResponse> RefreshToken()
        {
            AuthResponse response = new AuthResponse();

            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("RefreshToken is empty");
            }

            var isValid = authRepository.IsRefreshTokenValid(refreshToken);
            if (!isValid)
                return BadRequest("RefreshToken is invalid");
            var currentUser = authRepository.FindUserByToken(refreshToken);
            if (currentUser == null)
                return BadRequest("User is not found");

            var token = tokenProvider.GenerateToken(currentUser);
            response.AccessToken = token.AccessToken;
            response.RefreshToken = token.RefreshToken.Token;

            authRepository.DisableUserToken(refreshToken);
            authRepository.InsertRefreshtoken(token.RefreshToken, currentUser.Email);
            
            Response.Cookies.Append("refreshToken", token.RefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = token.RefreshToken.Expires,
                Path = "/"
            });
            
            return Ok(new AuthResponse
            {
                AccessToken = token.AccessToken,
            });

        }   

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken != null)
            {
                authRepository.DisableUserToken(refreshToken);
                Response.Cookies.Delete("refreshToken");
            }
            
            return Ok();
        }

    }
}