using System;
using System.Threading.Tasks;
using System.Web;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using log4net;

namespace ContosoCommerce.Users.Services
{
    /// <summary>
    /// Audit logging implementation. Captures
    /// user identity and IP address from
    /// HttpContext.Current (breaks in .NET 8
    /// without IHttpContextAccessor).
    /// </summary>
    public class AuditService : IAuditService
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(AuditService));

        private readonly CommerceDbContext _db;

        public AuditService(
            CommerceDbContext context)
        {
            _db = context;
        }

        public async Task LogAsync(
            string entityType,
            int entityId,
            string action,
            string details)
        {
            var performedBy = "System";
            var ipAddress = "Unknown";

            var httpContext =
                HttpContext.Current;
            if (httpContext != null)
            {
                var userId =
                    httpContext.Items["UserId"];
                if (userId != null)
                {
                    performedBy =
                        userId.ToString();
                }

                try
                {
                    ipAddress = httpContext
                        .Request
                        .UserHostAddress;
                }
                catch (Exception ex)
                {
                    Log.Warn(
                        "Could not get IP",
                        ex);
                }
            }

            var entry = new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Details = details,
                PerformedBy = performedBy,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            _db.AuditLogs.Add(entry);
            await _db.SaveChangesAsync();

            Log.InfoFormat(
                "Audit: {0} {1} on {2}#{3}",
                action, performedBy,
                entityType, entityId);
        }
    }
}
