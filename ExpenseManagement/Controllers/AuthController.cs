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
        private readonly  IAuthRepository authRepository;
        private readonly IAuthService authService;
        
        public AuthController(IAuthRepository authRepository, IAuthService authService)
        {
            this.authRepository = authRepository;
            this.authService = authService;
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            var result = authService.RegisterUser(request.Email, request.Password, request.Role);
            if (!result.Success)
            {
                return BadRequest(new{Message= result.Message});
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
            if (!result.Success)
            {
                return BadRequest(new{Message= result.Message});
            }
            Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly =true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.Data.Expires,
                Path = "/"
            });

            return Ok(new AuthResponse
            {
                AccessToken = result.Data.AccessToken,
                RefreshToken = result.Data.RefreshToken
            });
        }

        [HttpPost("refresh-token")]
        public ActionResult<AuthResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            
            var result = authService.RefreshToken(refreshToken);
            if (!result.Success)
            {
                return BadRequest(new{Message= result.Message});
            }
            Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires =result.Data.Expires,
                Path = "/"
            });
            
            return Ok(new AuthResponse
            {
                AccessToken = result.Data.AccessToken,
                RefreshToken = result.Data.RefreshToken
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