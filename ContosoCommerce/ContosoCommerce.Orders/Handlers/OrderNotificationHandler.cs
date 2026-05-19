using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using ContosoCommerce.Core.Interfaces;
using log4net;

namespace ContosoCommerce.Orders.Handlers
{
    /// <summary>
    /// DelegatingHandler that sends email
    /// notifications on order status changes.
    /// Uses SmtpClient which is deprecated in
    /// .NET 8 (should use MailKit instead).
    /// </summary>
    public class OrderNotificationHandler
        : DelegatingHandler
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(
                    OrderNotificationHandler));

        protected override async
            Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken token)
        {
            var response = await base
                .SendAsync(request, token);

            if (IsOrderStatusUpdate(request)
                && response.IsSuccessStatusCode)
            {
                try
                {
                    await SendNotification(
                        request);
                }
                catch (Exception ex)
                {
                    Log.Error(
                        "Order notification "
                        + "failed", ex);
                }
            }

            return response;
        }

        private static bool IsOrderStatusUpdate(
            HttpRequestMessage request)
        {
            var path = request.RequestUri
                .AbsolutePath;
            return path.Contains(
                    "/api/orders/")
                && request.Method
                    == HttpMethod.Put;
        }

        #pragma warning disable 618
        private async Task SendNotification(
            HttpRequestMessage request)
        {
            var smtpHost = ConfigurationManager
                .AppSettings["SmtpHost"]
                ?? "localhost";
            var smtpPort = int.Parse(
                ConfigurationManager
                    .AppSettings["SmtpPort"]
                    ?? "25");

            Log.InfoFormat(
                "Sending order notification "
                + "via {0}:{1}",
                smtpHost, smtpPort);

            using (var client =
                new SmtpClient(
                    smtpHost, smtpPort))
            {
                client.EnableSsl = false;

                var message = new MailMessage
                {
                    From = new MailAddress(
                        "noreply@contoso.com",
                        "Contoso Commerce"),
                    Subject =
                        "Order Status Update",
                    Body =
                        "Your order status has "
                        + "been updated. "
                        + "Please check your "
                        + "account for details.",
                    IsBodyHtml = false
                };

                message.To.Add(
                    "customer@contoso.com");

                try
                {
                    client.Send(message);
                    Log.Info(
                        "Notification sent.");
                }
                catch (SmtpException ex)
                {
                    Log.Warn(
                        "SMTP send failed "
                        + "(mocked): "
                        + ex.Message);
                }
            }
        }
        #pragma warning restore 618
    }
}
