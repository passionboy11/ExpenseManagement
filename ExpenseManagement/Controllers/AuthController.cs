using ExpenseManagement.DTO;
using ExpenseManagement.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManagement.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccess dataAccess;
        private readonly TokenProvider tokenProvider;

        // Controller depends on DataAccess for db operation
        // This is injected via Dependency Injection (DI)
        public AuthController(DataAccess dataAccess, TokenProvider tokenProvider)
        {
            this.dataAccess = dataAccess;
            this.tokenProvider = tokenProvider;
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;
            var result = dataAccess.RegisterUser(request.Email, hashedPassword,role);
            if (result)
            {

                return Ok(new {message = "User Registered Successfully"});

                return Ok("User created successfully");

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

            var user = dataAccess.FindUserByEmail(request.Email);
            if (user == null)
                return BadRequest("User is not found");

            var verifyPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!verifyPassword)
                return BadRequest("Wrong Password");

            // Generate Access token
            var token = tokenProvider.GenerateToken(user);
            response.AccessToken = token.AccessToken;

            // Generate refresh token
            response.RefreshToken = token.RefreshToken.Token;

            dataAccess.DisableUserTokenByEmail(request.Email);
            dataAccess.InsertRefreshtoken(token.RefreshToken, request.Email);

            return Ok(response);
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

            var isValid = dataAccess.IsRefreshTokenValid(refreshToken);
            if (!isValid)
                return BadRequest("RefreshToken is invalid");
            var currentUser = dataAccess.FindUserByToken(refreshToken);
            if (currentUser == null)
                return BadRequest("User is not found");

            var token = tokenProvider.GenerateToken(currentUser);
            response.AccessToken = token.AccessToken;
            response.RefreshToken = token.RefreshToken.Token;

            dataAccess.DisableUserToken(refreshToken);
            dataAccess.InsertRefreshtoken(token.RefreshToken, currentUser.Email);
            return Ok(response);

        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken != null)
            {
                dataAccess.DisableUserToken(refreshToken);
            }

            return Ok();
        }

    }
}