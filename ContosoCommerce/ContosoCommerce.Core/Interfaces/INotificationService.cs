using System.Threading.Tasks;

namespace ContosoCommerce.Core.Interfaces
{
    /// <summary>
    /// Sends notifications (email, etc.) to users.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends an email notification.
        /// </summary>
        Task SendEmailAsync(
            string toAddress,
            string subject,
            string body);

        /// <summary>
        /// Sends a low-stock alert notification.
        /// </summary>
        Task SendStockAlertAsync(
            string productName,
            int currentStock,
            int threshold);

        /// <summary>
        /// Sends an order status update email.
        /// </summary>
        Task SendOrderStatusEmailAsync(
            string toAddress,
            int orderId,
            string newStatus);
    }
}
