using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly IAuthService authService;

        public AuthController(IAuthRepository authRepository, IAuthService authService)
        {
            this.authRepository = authRepository;
            this.authService = authService;
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            var result = authService.RegisterUser(request.Email, request.Password, request.Role ?? string.Empty);
            if (!result.Success)
            {
                return this.ToErrorResult(result.ErrorType, result.Message);
            }
            return Ok(new
            {
                Message = result.Message
            });
        }

        [HttpPost("login")]
        public ActionResult<AuthResponse> Login(AuthRequest request)
        {
            var result = authService.Login(request.Email, request.Password);
            if (!result.Success || result.Data is null)
            {
                return this.ToErrorResult(result.ErrorType, result.Message);
            }

            var data = result.Data;

            if (string.IsNullOrEmpty(data.RefreshToken))
            {
                // Shouldn't happen on a successful login - treat as a server-side failure
                // rather than pass a null value into the cookie writer.
                return this.ToErrorResult(ErrorType.ServerError, "Login succeeded but no refresh token was issued.");
            }

            Response.Cookies.Append("refreshToken", data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = data.Expires,
                Path = "/"
            });

            return Ok(new AuthResponse
            {
                AccessToken = data.AccessToken
            });
        }

        [HttpPost("refresh-token")]
        public ActionResult<AuthResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return this.ToErrorResult(ErrorType.Unauthorized, "No refresh token was provided.");
            }

            var result = authService.RefreshToken(refreshToken);
            if (!result.Success || result.Data is null)
            {
                return this.ToErrorResult(result.ErrorType, result.Message);
            }

            var data = result.Data;

            if (string.IsNullOrEmpty(data.RefreshToken))
            {
                return this.ToErrorResult(ErrorType.ServerError, "Token refresh succeeded but no refresh token was issued.");
            }

            Response.Cookies.Append("refreshToken", data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = data.Expires,
                Path = "/"
            });

            return Ok(new AuthResponse
            {
                AccessToken = data.AccessToken
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