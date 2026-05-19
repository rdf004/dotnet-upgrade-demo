using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoCommerce.Inventory.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/stock")]
    public class StockController
        : ControllerBase
    {
        private readonly
            IInventoryService _svc;

        public StockController(
            IInventoryService svc)
        {
            _svc = svc;
        }

        [HttpGet(
            "{productId:int}/level")]
        public async Task<IActionResult>
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

        [HttpPut(
            "{productId:int}/adjust")]
        public async Task<IActionResult>
            Adjust(
                int productId,
                StockAdjustRequest request)
        {
            if (request == null)
                return BadRequest();

            try
            {
                await _svc
                    .AdjustStockAsync(
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

        [HttpGet("low")]
        public async Task<IActionResult>
            GetLowStock(
                int threshold = 10)
        {
            var products = await _svc
                .GetLowStockProductsAsync(
                    threshold);
            return Ok(
                ApiResponse<
                    IList<ProductDto>>
                    .Ok(products));
        }
    }

    [System.Serializable]
    public class StockAdjustRequest
    {
        public int NewQuantity { get; set; }
    }
}
