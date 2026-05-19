using System.Threading.Tasks;
using System.Web.Http;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Users.Filters;
using log4net;

namespace ContosoCommerce.Inventory.Controllers
{
    /// <summary>
    /// CRUD for products with image upload.
    /// Stores images as byte[] in the database.
    /// </summary>
    [TokenAuthorize]
    [RoutePrefix("api/products")]
    public class ProductsController
        : ApiController
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(ProductsController));

        private readonly IInventoryService _svc;

        public ProductsController(
            IInventoryService inventoryService)
        {
            _svc = inventoryService;
        }

        /// <summary>
        /// GET api/products
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult>
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

        /// <summary>
        /// GET api/products/5
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult>
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

        /// <summary>
        /// GET api/products/5/image
        /// </summary>
        [HttpGet]
        [Route("{id:int}/image")]
        public async Task<IHttpActionResult>
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

        /// <summary>
        /// POST api/products
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult>
            Create(
                CreateProductRequest request)
        {
            if (request == null
                || !ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            try
            {
                var product = await _svc
                    .CreateProductAsync(request);
                return Created(
                    string.Format(
                        "api/products/{0}",
                        product.Id),
                    ApiResponse<ProductDto>
                        .Ok(product,
                            "Product created."));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(
                    ex.Message);
            }
        }

        /// <summary>
        /// PUT api/products/5
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult>
            Update(
                int id,
                UpdateProductRequest request)
        {
            if (request == null
                || !ModelState.IsValid)
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
                            "Product updated."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// DELETE api/products/5
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IHttpActionResult>
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
