using System.Threading.Tasks;
using ContosoCommerce.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Users.Services
{
    public class MockNotificationService
        : INotificationService
    {
        private readonly
            ILogger<
                MockNotificationService>
            _log;

        public MockNotificationService(
            ILogger<
                MockNotificationService>
                logger)
        {
            _log = logger;
        }

        public Task SendEmailAsync(
            string toAddress,
            string subject,
            string body)
        {
            _log.LogInformation(
                "Mock email to {To}:"
                + " {Subject}",
                toAddress, subject);
            return Task.CompletedTask;
        }

        public Task SendStockAlertAsync(
            string productName,
            int currentStock,
            int threshold)
        {
            _log.LogWarning(
                "Mock stock alert:"
                + " {Name} at {Qty}"
                + " (threshold {Th})",
                productName,
                currentStock,
                threshold);
            return Task.CompletedTask;
        }

        public Task
            SendOrderStatusEmailAsync(
                string toAddress,
                int orderId,
                string newStatus)
        {
            _log.LogInformation(
                "Mock order status"
                + " email to {To}:"
                + " order {Id} -> {St}",
                toAddress,
                orderId,
                newStatus);
            return Task.CompletedTask;
        }
    }
}
