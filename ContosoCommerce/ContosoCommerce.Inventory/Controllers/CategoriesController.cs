using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoCommerce.Inventory.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController
        : ControllerBase
    {
        private readonly
            IInventoryService _svc;

        public CategoriesController(
            IInventoryService svc)
        {
            _svc = svc;
        }

        [HttpGet("")]
        public async Task<IActionResult>
            GetAll()
        {
            var categories = await _svc
                .GetCategoriesAsync();
            return Ok(
                ApiResponse<
                    IList<CategoryDto>>
                    .Ok(categories));
        }

        [HttpPost("")]
        public async Task<IActionResult>
            Create(
                CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
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
