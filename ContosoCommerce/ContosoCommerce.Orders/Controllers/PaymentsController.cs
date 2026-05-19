using System.Threading.Tasks;
using System.Web.Http;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Users.Filters;
using log4net;

namespace ContosoCommerce.Orders.Controllers
{
    /// <summary>
    /// Mock payment processing endpoint.
    /// </summary>
    [TokenAuthorize]
    [RoutePrefix("api/payments")]
    public class PaymentsController
        : ApiController
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(PaymentsController));

        private readonly IOrderService _svc;

        public PaymentsController(
            IOrderService orderService)
        {
            _svc = orderService;
        }

        /// <summary>
        /// POST api/payments/order/5
        /// </summary>
        [HttpPost]
        [Route("order/{orderId:int}")]
        public async Task<IHttpActionResult>
            ProcessPayment(
                int orderId,
                PaymentRequest request)
        {
            if (request == null
                || !ModelState.IsValid)
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
                            "Payment "
                            + "processed."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
