namespace ContosoCommerce.Core.Enums
{
    /// <summary>
    /// Lifecycle states for a commerce order.
    /// </summary>
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        PaymentReceived = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Refunded = 6
    }
}
