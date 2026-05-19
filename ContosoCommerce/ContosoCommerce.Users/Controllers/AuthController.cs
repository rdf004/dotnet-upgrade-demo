using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Users.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController
        : ControllerBase
    {
        private readonly
            ILogger<AuthController> _log;
        private readonly IUserService _svc;

        public AuthController(
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _svc = userService;
            _log = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult>
            Login(LoginRequest request)
        {
            if (request == null
                || string.IsNullOrEmpty(
                    request.Email)
                || string.IsNullOrEmpty(
                    request.Password))
            {
                return BadRequest(
                    "Email and password"
                    + " are required.");
            }

            _log.LogInformation(
                "Login attempt: {Email}",
                request.Email);

            var result = await _svc
                .AuthenticateAsync(
                    request.Email,
                    request.Password);

            if (!result.Success)
            {
                return Unauthorized();
            }

            return Ok(
                ApiResponse<AuthResult>
                    .Ok(result,
                        "Login successful."));
        }
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
