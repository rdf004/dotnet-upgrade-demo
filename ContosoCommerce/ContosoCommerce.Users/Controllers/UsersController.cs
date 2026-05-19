using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Users.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UsersController
        : ControllerBase
    {
        private readonly
            ILogger<UsersController> _log;
        private readonly IUserService _svc;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _svc = userService;
            _log = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult>
            GetAll(
                int page = 1,
                int pageSize = 10)
        {
            var users = await _svc
                .GetAllUsersAsync(
                    page, pageSize);
            var count = await _svc
                .GetUserCountAsync();

            var result =
                new PagedResult<UserDto>
                {
                    Items = users,
                    TotalCount = count,
                    Page = page,
                    PageSize = pageSize
                };

            return Ok(
                ApiResponse<
                    PagedResult<UserDto>>
                    .Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult>
            Get(int id)
        {
            try
            {
                var user = await _svc
                    .GetUserAsync(id);
                return Ok(
                    ApiResponse<UserDto>
                        .Ok(user));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("")]
        public async Task<IActionResult>
            Create(
                CreateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            try
            {
                var user = await _svc
                    .CreateUserAsync(request);
                return Created(
                    string.Format(
                        "api/users/{0}",
                        user.Id),
                    ApiResponse<UserDto>
                        .Ok(user,
                            "User created."));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(
                    ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult>
            Update(
                int id,
                UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            try
            {
                var user = await _svc
                    .UpdateUserAsync(
                        id, request);
                return Ok(
                    ApiResponse<UserDto>
                        .Ok(user,
                            "User updated."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult>
            Delete(int id)
        {
            try
            {
                await _svc
                    .DeleteUserAsync(id);
                return Ok(
                    ApiResponse<string>.Ok(
                        null,
                        "User deleted."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
