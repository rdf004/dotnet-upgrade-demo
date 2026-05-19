using System.Threading.Tasks;
using System.Web.Http;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Users.Filters;
using log4net;

namespace ContosoCommerce.Users.Controllers
{
    /// <summary>
    /// CRUD endpoints for user management.
    /// </summary>
    [TokenAuthorize]
    [RoutePrefix("api/users")]
    public class UsersController
        : ApiController
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(UsersController));

        private readonly IUserService _svc;

        public UsersController(
            IUserService userService)
        {
            _svc = userService;
        }

        /// <summary>
        /// GET api/users?page=1&amp;pageSize=10
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult>
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

        /// <summary>
        /// GET api/users/5
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult>
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

        /// <summary>
        /// POST api/users
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult>
            Create(
                CreateUserRequest request)
        {
            if (request == null
                || !ModelState.IsValid)
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

        /// <summary>
        /// PUT api/users/5
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult>
            Update(
                int id,
                UpdateUserRequest request)
        {
            if (request == null
                || !ModelState.IsValid)
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

        /// <summary>
        /// DELETE api/users/5
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult>
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
