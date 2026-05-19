using System.Threading.Tasks;
using System.Web.Http;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Users.Filters;

namespace ContosoCommerce.Inventory.Controllers
{
    /// <summary>
    /// CRUD for hierarchical product categories.
    /// </summary>
    [TokenAuthorize]
    [RoutePrefix("api/categories")]
    public class CategoriesController
        : ApiController
    {
        private readonly IInventoryService _svc;

        public CategoriesController(
            IInventoryService inventoryService)
        {
            _svc = inventoryService;
        }

        /// <summary>
        /// GET api/categories
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult>
            GetAll()
        {
            var categories = await _svc
                .GetCategoriesAsync();
            return Ok(
                ApiResponse<
                    System.Collections.Generic
                        .IList<CategoryDto>>
                    .Ok(categories));
        }

        /// <summary>
        /// POST api/categories
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult>
            Create(
                CreateCategoryRequest request)
        {
            if (request == null
                || !ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            var category = await _svc
                .CreateCategoryAsync(request);
            return Created(
                string.Format(
                    "api/categories/{0}",
                    category.Id),
                ApiResponse<CategoryDto>
                    .Ok(category,
                        "Category created."));
        }
    }
}
