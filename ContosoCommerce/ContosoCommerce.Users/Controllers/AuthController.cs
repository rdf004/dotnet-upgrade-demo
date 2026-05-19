using System.Threading.Tasks;
using System.Web.Http;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Interfaces;
using log4net;

namespace ContosoCommerce.Users.Controllers
{
    /// <summary>
    /// Authentication endpoint. Returns a
    /// custom DB-stored token (not JWT).
    /// </summary>
    [RoutePrefix("api/auth")]
    public class AuthController
        : ApiController
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(AuthController));

        private readonly IUserService _svc;

        public AuthController(
            IUserService userService)
        {
            _svc = userService;
        }

        /// <summary>
        /// POST api/auth/login
        /// </summary>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IHttpActionResult>
            Login(LoginRequest request)
        {
            if (request == null
                || string.IsNullOrEmpty(
                    request.Email)
                || string.IsNullOrEmpty(
                    request.Password))
            {
                return BadRequest(
                    "Email and password "
                    + "are required.");
            }

            Log.InfoFormat(
                "Login attempt: {0}",
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

    /// <summary>
    /// Login request payload.
    /// </summary>
    [System.Serializable]
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
