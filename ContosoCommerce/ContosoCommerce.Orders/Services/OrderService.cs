using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Orders.Services
{
    public class OrderService
        : IOrderService
    {
        private readonly
            ILogger<OrderService> _log;
        private readonly CommerceDbContext _db;
        private readonly
            IInventoryService _inv;
        private readonly IUserService _users;
        private readonly IAuditService _audit;
        private readonly
            INotificationService _notify;
        private readonly string _paymentUrl;

        public OrderService(
            CommerceDbContext context,
            IInventoryService invService,
            IUserService userService,
            IAuditService auditService,
            INotificationService notifySvc,
            IConfiguration config,
            ILogger<OrderService> logger)
        {
            _db = context;
            _inv = invService;
            _users = userService;
            _audit = auditService;
            _notify = notifySvc;
            _log = logger;

            _paymentUrl = config[
                "PaymentSettings:GatewayUrl"]
                ?? "https://pay.contoso.com"
                    + "/api";
        }

        public async Task<OrderDto>
            GetOrderAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(
                        i => i.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(
                    o => o.Id == orderId);

            if (order == null)
            {
                throw
                    new EntityNotFoundException(
                        "Order", orderId);
            }
            return MapToDto(order);
        }

        public async Task<IList<OrderDto>>
            GetUserOrdersAsync(
                int userId,
                int page,
                int pageSize)
        {
            var orders = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(
                        i => i.Product)
                .Include(o => o.User)
                .Where(
                    o => o.UserId == userId)
                .OrderByDescending(
                    o => o.OrderDate)
                .Skip(
                    (page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return orders
                .Select(MapToDto)
                .ToList();
        }

        public async Task<OrderDto>
            CreateOrderAsync(
                CreateOrderRequest request)
        {
            _log.LogInformation(
                "Creating order for user"
                + " {UserId}",
                request.UserId);

            var user = await _users
                .GetUserAsync(
                    request.UserId);

            await using var txn = await _db
                .Database
                .BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId =
                        request.UserId,
                    Status =
                        OrderStatus.Pending,
                    ShippingAddress =
                        request
                            .ShippingAddress,
                    OrderDate =
                        DateTime.UtcNow,
                    TotalAmount = 0
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                decimal total = 0;

                foreach (var itemReq
                    in request.Items)
                {
                    var product = await _inv
                        .GetProductAsync(
                            itemReq.ProductId);

                    var reserved = await _inv
                        .ReserveStockAsync(
                            itemReq.ProductId,
                            itemReq.Quantity);

                    if (!reserved)
                    {
                        throw new
                            BusinessRuleException(
                                "InsufficientStock",
                                string.Format(
                                    "Not enough"
                                    + " stock"
                                    + " for {0}.",
                                    product
                                        .Name));
                    }

                    var lineTotal =
                        product.Price
                        * itemReq.Quantity;

                    var item = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId =
                            itemReq.ProductId,
                        Quantity =
                            itemReq.Quantity,
                        UnitPrice =
                            product.Price,
                        LineTotal = lineTotal
                    };

                    _db.OrderItems.Add(item);
                    total += lineTotal;
                }

                order.TotalAmount = total;
                await _db.SaveChangesAsync();
                await txn.CommitAsync();

                await _audit.LogAsync(
                    "Order", order.Id,
                    "Create",
                    string.Format(
                        "Order created"
                        + " for ${0:N2}",
                        total));

                return await GetOrderAsync(
                    order.Id);
            }
            catch
            {
                await txn.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto>
            UpdateOrderStatusAsync(
                int orderId,
                OrderStatus newStatus)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(
                    o => o.Id == orderId);

            if (order == null)
            {
                throw
                    new EntityNotFoundException(
                        "Order", orderId);
            }

            var oldStatus = order.Status;
            order.Status = newStatus;
            order.UpdatedAt =
                DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                "Order", orderId,
                "StatusChange",
                string.Format(
                    "{0} -> {1}",
                    oldStatus, newStatus));

            if (newStatus
                == OrderStatus.Cancelled)
            {
                await ReleaseOrderStock(
                    orderId);
            }

            await _notify
                .SendOrderStatusEmailAsync(
                    order.User.Email,
                    orderId,
                    newStatus.ToString());

            return await GetOrderAsync(
                orderId);
        }

        public async Task<PaymentResultDto>
            ProcessPaymentAsync(
                int orderId,
                PaymentRequest request)
        {
            _log.LogInformation(
                "Processing payment for"
                + " order {Id} at {Url}",
                orderId, _paymentUrl);

            var order = await _db.Orders
                .FindAsync(orderId);
            if (order == null)
            {
                throw
                    new EntityNotFoundException(
                        "Order", orderId);
            }

            var result =
                await SimulatePaymentAsync(
                    request, order);

            var payment = new Payment
            {
                OrderId = orderId,
                Amount =
                    result.AmountCharged,
                TransactionId =
                    result.TransactionId,
                Status = result.Success
                    ? "Completed"
                    : "Failed",
                PaymentMethod =
                    "CreditCard",
                ProcessedAt =
                    DateTime.UtcNow,
                ErrorMessage =
                    result.ErrorMessage
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            if (result.Success)
            {
                order.Status =
                    OrderStatus
                        .PaymentReceived;
                await _db.SaveChangesAsync();
            }

            return result;
        }

        public async Task<int>
            GetOrderCountAsync()
        {
            return await _db.Orders
                .CountAsync();
        }

        public async Task<decimal>
            GetTotalRevenueAsync()
        {
            return await _db.Orders
                .Where(o =>
                    o.Status
                        != OrderStatus
                            .Cancelled
                    && o.Status
                        != OrderStatus
                            .Refunded)
                .SumAsync(
                    o => o.TotalAmount);
        }

        private async
            Task<PaymentResultDto>
            SimulatePaymentAsync(
                PaymentRequest request,
                Order order)
        {
            await Task.Delay(500);

            var txnId = string.Format(
                "TXN-{0}-{1}",
                DateTime.UtcNow
                    .ToString("yyyyMMdd"),
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 8));

            _log.LogInformation(
                "Payment simulated: {TxnId}",
                txnId);

            return new PaymentResultDto
            {
                Success = true,
                TransactionId = txnId,
                AmountCharged =
                    order.TotalAmount,
                ErrorMessage = null
            };
        }

        private async Task
            ReleaseOrderStock(int orderId)
        {
            var items = await _db.OrderItems
                .Where(
                    i => i.OrderId == orderId)
                .ToListAsync();

            foreach (var item in items)
            {
                await _inv
                    .ReleaseStockAsync(
                        item.ProductId,
                        item.Quantity);
            }
        }

        private static OrderDto MapToDto(
            Order o)
        {
            return new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserEmail =
                    o.User != null
                        ? o.User.Email
                        : null,
                UserName =
                    o.User != null
                        ? o.User.FullName
                        : null,
                Status = o.Status,
                TotalAmount =
                    o.TotalAmount,
                ShippingAddress =
                    o.ShippingAddress,
                OrderDate = o.OrderDate,
                Items = o.Items
                    .Select(i =>
                        new OrderItemDto
                        {
                            ProductId =
                                i.ProductId,
                            ProductName =
                                i.Product
                                    != null
                                    ? i.Product
                                        .Name
                                    : null,
                            Quantity =
                                i.Quantity,
                            UnitPrice =
                                i.UnitPrice,
                            LineTotal =
                                i.LineTotal
                        })
                    .ToList()
            };
        }
    }
}
