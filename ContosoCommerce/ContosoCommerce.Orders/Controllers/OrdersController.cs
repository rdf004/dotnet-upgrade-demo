using System.Threading.Tasks;
using System.Web.Http;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Users.Filters;
using log4net;

namespace ContosoCommerce.Orders.Controllers
{
    /// <summary>
    /// Order management endpoints.
    /// </summary>
    [TokenAuthorize]
    [RoutePrefix("api/orders")]
    public class OrdersController
        : ApiController
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(OrdersController));

        private readonly IOrderService _svc;

        public OrdersController(
            IOrderService orderService)
        {
            _svc = orderService;
        }

        /// <summary>
        /// GET api/orders/5
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult>
            Get(int id)
        {
            try
            {
                var order = await _svc
                    .GetOrderAsync(id);
                return Ok(
                    ApiResponse<OrderDto>
                        .Ok(order));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// GET api/orders/user/3
        /// </summary>
        [HttpGet]
        [Route("user/{userId:int}")]
        public async Task<IHttpActionResult>
            GetUserOrders(
                int userId,
                int page = 1,
                int pageSize = 10)
        {
            var orders = await _svc
                .GetUserOrdersAsync(
                    userId, page, pageSize);
            return Ok(
                ApiResponse<
                    System.Collections.Generic
                        .IList<OrderDto>>
                    .Ok(orders));
        }

        /// <summary>
        /// POST api/orders
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult>
            Create(
                CreateOrderRequest request)
        {
            if (request == null
                || !ModelState.IsValid)
            {
                return BadRequest(
                    ModelState);
            }

            try
            {
                var order = await _svc
                    .CreateOrderAsync(request);
                return Created(
                    string.Format(
                        "api/orders/{0}",
                        order.Id),
                    ApiResponse<OrderDto>
                        .Ok(order,
                            "Order created."));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(
                    ex.Message);
            }
        }

        /// <summary>
        /// PUT api/orders/5/status
        /// </summary>
        [HttpPut]
        [Route("{id:int}/status")]
        public async Task<IHttpActionResult>
            UpdateStatus(
                int id,
                OrderStatusRequest request)
        {
            if (request == null)
                return BadRequest();

            try
            {
                var order = await _svc
                    .UpdateOrderStatusAsync(
                        id, request.Status);
                return Ok(
                    ApiResponse<OrderDto>
                        .Ok(order,
                            "Status updated."));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }

    [System.Serializable]
    public class OrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}
