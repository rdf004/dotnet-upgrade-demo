using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Orders.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/orders")]
    public class OrdersController
        : ControllerBase
    {
        private readonly
            ILogger<OrdersController> _log;
        private readonly IOrderService _svc;

        public OrdersController(
            IOrderService orderService,
            ILogger<OrdersController> logger)
        {
            _svc = orderService;
            _log = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult>
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

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult>
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
                    IList<OrderDto>>
                    .Ok(orders));
        }

        [HttpPost("")]
        public async Task<IActionResult>
            Create(
                CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
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

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult>
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
