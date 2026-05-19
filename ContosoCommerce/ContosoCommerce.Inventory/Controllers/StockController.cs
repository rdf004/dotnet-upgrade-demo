using System.Threading.Tasks;
using System.Web.Http;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Users.Filters;

namespace ContosoCommerce.Inventory.Controllers
{
    /// <summary>
    /// Manages stock levels and alerts.
    /// </summary>
    [TokenAuthorize]
    [RoutePrefix("api/stock")]
    public class StockController
        : ApiController
    {
        private readonly IInventoryService _svc;

        public StockController(
            IInventoryService inventoryService)
        {
            _svc = inventoryService;
        }

        /// <summary>
        /// GET api/stock/5/level
        /// </summary>
        [HttpGet]
        [Route("{productId:int}/level")]
        public async Task<IHttpActionResult>
            GetLevel(int productId)
        {
            try
            {
                var level = await _svc
                    .GetStockLevelAsync(
                        productId);
                return Ok(
                    ApiResponse<StockLevel>
                        .Ok(level));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// PUT api/stock/5/adjust
        /// </summary>
        [HttpPut]
        [Route("{productId:int}/adjust")]
        public async Task<IHttpActionResult>
            Adjust(
                int productId,
                StockAdjustRequest request)
        {
            if (request == null)
                return BadRequest();

            try
            {
                await _svc.AdjustStockAsync(
                    productId,
                    request.NewQuantity);
                return Ok(
                    ApiResponse<string>.Ok(
                        null,
                        "Stock adjusted."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(
                    ex.Message);
            }
        }

        /// <summary>
        /// GET api/stock/low?threshold=10
        /// </summary>
        [HttpGet]
        [Route("low")]
        public async Task<IHttpActionResult>
            GetLowStock(int threshold = 10)
        {
            var products = await _svc
                .GetLowStockProductsAsync(
                    threshold);
            return Ok(
                ApiResponse<
                    System.Collections.Generic
                        .IList<ProductDto>>
                    .Ok(products));
        }
    }

    [System.Serializable]
    public class StockAdjustRequest
    {
        public int NewQuantity { get; set; }
    }
}
