using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Orders.Handlers
{
    public class OrderNotificationHandler
    {
        private readonly
            ILogger<
                OrderNotificationHandler>
            _log;
        private readonly string _smtpHost;
        private readonly int _smtpPort;

        public OrderNotificationHandler(
            IConfiguration config,
            ILogger<
                OrderNotificationHandler>
                logger)
        {
            _log = logger;
            _smtpHost = config[
                "SmtpSettings:Host"]
                ?? "localhost";
            var portStr = config[
                "SmtpSettings:Port"];
            _smtpPort = int.TryParse(
                portStr, out var p) ? p : 25;
        }

        #pragma warning disable CS0618
        public async Task
            SendNotificationAsync(
                string toEmail,
                int orderId,
                string status)
        {
            _log.LogInformation(
                "Sending order notification"
                + " via {Host}:{Port}",
                _smtpHost, _smtpPort);

            try
            {
                using var client =
                    new SmtpClient(
                        _smtpHost,
                        _smtpPort);
                client.EnableSsl = false;

                var message = new MailMessage
                {
                    From =
                        new MailAddress(
                            "noreply"
                            + "@contoso.com",
                            "Contoso"
                            + " Commerce"),
                    Subject =
                        "Order Status"
                        + " Update",
                    Body =
                        "Your order status"
                        + " has been updated."
                        + " Please check your"
                        + " account.",
                    IsBodyHtml = false
                };

                message.To.Add(toEmail);

                await Task.Run(
                    () => client
                        .Send(message));
                _log.LogInformation(
                    "Notification sent.");
            }
            catch (SmtpException ex)
            {
                _log.LogWarning(
                    "SMTP send failed"
                    + " (mocked): {Msg}",
                    ex.Message);
            }
        }
        #pragma warning restore CS0618
    }
}
