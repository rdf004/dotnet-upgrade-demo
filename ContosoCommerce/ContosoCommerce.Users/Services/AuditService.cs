using System;
using System.Threading.Tasks;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Users.Services
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService>
            _log;
        private readonly CommerceDbContext _db;
        private readonly IHttpContextAccessor
            _httpCtx;

        public AuditService(
            CommerceDbContext context,
            IHttpContextAccessor accessor,
            ILogger<AuditService> logger)
        {
            _db = context;
            _httpCtx = accessor;
            _log = logger;
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
                _httpCtx.HttpContext;
            if (httpContext != null)
            {
                var userId =
                    httpContext
                        .Items["UserId"];
                if (userId != null)
                {
                    performedBy =
                        userId.ToString();
                }

                try
                {
                    var remote = httpContext
                        .Connection
                        .RemoteIpAddress;
                    if (remote != null)
                    {
                        ipAddress =
                            remote.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _log.LogWarning(
                        ex,
                        "Could not get IP");
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

            _log.LogInformation(
                "Audit: {Action} {By}"
                + " on {Type}#{Id}",
                action, performedBy,
                entityType, entityId);
        }
    }
}
