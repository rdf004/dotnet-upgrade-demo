using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Orders.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController
        : ControllerBase
    {
        private readonly
            ILogger<PaymentsController> _log;
        private readonly IOrderService _svc;

        public PaymentsController(
            IOrderService orderService,
            ILogger<PaymentsController> log)
        {
            _svc = orderService;
            _log = log;
        }

        [HttpPost("order/{orderId:int}")]
        public async Task<IActionResult>
            ProcessPayment(
                int orderId,
                PaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            try
            {
                var result = await _svc
                    .ProcessPaymentAsync(
                        orderId, request);

                if (!result.Success)
                {
                    return BadRequest(
                        result.ErrorMessage);
                }

                return Ok(
                    ApiResponse<
                        PaymentResultDto>
                        .Ok(result,
                            "Payment"
                            + " processed."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
