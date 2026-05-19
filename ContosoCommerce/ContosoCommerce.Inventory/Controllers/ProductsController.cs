using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Inventory.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class ProductsController
        : ControllerBase
    {
        private readonly
            ILogger<ProductsController> _log;
        private readonly
            IInventoryService _svc;

        public ProductsController(
            IInventoryService svc,
            ILogger<ProductsController> log)
        {
            _svc = svc;
            _log = log;
        }

        [HttpGet("")]
        public async Task<IActionResult>
            GetAll(
                int page = 1,
                int pageSize = 10,
                int? categoryId = null)
        {
            var products = await _svc
                .GetProductsAsync(
                    page, pageSize,
                    categoryId);
            var count = await _svc
                .GetProductCountAsync();

            var result =
                new PagedResult<ProductDto>
                {
                    Items = products,
                    TotalCount = count,
                    Page = page,
                    PageSize = pageSize
                };

            return Ok(
                ApiResponse<
                    PagedResult<ProductDto>>
                    .Ok(result));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult>
            Get(int id)
        {
            try
            {
                var product = await _svc
                    .GetProductAsync(id);
                return Ok(
                    ApiResponse<ProductDto>
                        .Ok(product));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id:int}/image")]
        public async Task<IActionResult>
            GetImage(int id)
        {
            try
            {
                var data = await _svc
                    .GetProductImageAsync(id);
                if (data == null)
                    return NotFound();
                return Ok(data);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("")]
        public async Task<IActionResult>
            Create(
                CreateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            try
            {
                var product = await _svc
                    .CreateProductAsync(
                        request);
                return Created(
                    string.Format(
                        "api/products/{0}",
                        product.Id),
                    ApiResponse<ProductDto>
                        .Ok(product,
                            "Product"
                            + " created."));
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
                UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            try
            {
                var product = await _svc
                    .UpdateProductAsync(
                        id, request);
                return Ok(
                    ApiResponse<ProductDto>
                        .Ok(product,
                            "Product"
                            + " updated."));
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
                    .DeleteProductAsync(id);
                return Ok(
                    ApiResponse<string>.Ok(
                        null,
                        "Product deleted."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
