using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCommerce.Core.Enums;

namespace ContosoCommerce.Core.Interfaces
{
    /// <summary>
    /// Manages orders and payment processing.
    /// </summary>
    public interface IOrderService
    {
        Task<OrderDto> GetOrderAsync(int orderId);

        Task<IList<OrderDto>> GetUserOrdersAsync(
            int userId, int page, int pageSize);

        Task<OrderDto> CreateOrderAsync(
            CreateOrderRequest request);

        Task<OrderDto> UpdateOrderStatusAsync(
            int orderId, OrderStatus newStatus);

        Task<PaymentResultDto> ProcessPaymentAsync(
            int orderId,
            PaymentRequest request);

        Task<int> GetOrderCountAsync();

        Task<decimal> GetTotalRevenueAsync();
    }

    [System.Serializable]
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }

        public System.DateTime OrderDate
        {
            get; set;
        }

        public IList<OrderItemDto> Items
        {
            get; set;
        }
    }

    [System.Serializable]
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    [System.Serializable]
    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; }

        public IList<OrderItemRequest> Items
        {
            get; set;
        }
    }

    [System.Serializable]
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    [System.Serializable]
    public class PaymentRequest
    {
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string Cvv { get; set; }
        public decimal Amount { get; set; }
    }

    [System.Serializable]
    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public decimal AmountCharged { get; set; }
    }
}
