using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Inventory.Services
{
    public class StockMonitorWorker
        : BackgroundService
    {
        private readonly
            ILogger<StockMonitorWorker> _log;
        private readonly
            IServiceScopeFactory _scopeFactory;
        private readonly int _threshold;
        private readonly TimeSpan _interval;

        public StockMonitorWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<StockMonitorWorker> log,
            int threshold = 10,
            double intervalMinutes = 30)
        {
            _scopeFactory = scopeFactory;
            _log = log;
            _threshold = threshold;
            _interval = TimeSpan
                .FromMinutes(intervalMinutes);
        }

        protected override async Task
            ExecuteAsync(
                CancellationToken stoppingToken)
        {
            _log.LogInformation(
                "Stock monitor starting."
                + " Interval: {Interval}",
                _interval);

            while (!stoppingToken
                .IsCancellationRequested)
            {
                try
                {
                    await CheckStockLevels();
                }
                catch (Exception ex)
                {
                    _log.LogError(
                        ex,
                        "Stock check failed");
                }

                await Task.Delay(
                    _interval,
                    stoppingToken);
            }

            _log.LogInformation(
                "Stock monitor stopping.");
        }

        private async Task CheckStockLevels()
        {
            _log.LogDebug(
                "Running stock level check");

            using var scope = _scopeFactory
                .CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<
                    CommerceDbContext>();
            var notify =
                scope.ServiceProvider
                    .GetRequiredService<
                        INotificationService>();

            var lowStock = await db.Products
                .Where(p => p.IsActive
                    && p.StockQuantity
                        <= _threshold)
                .ToListAsync();

            _log.LogInformation(
                "Found {Count}"
                + " low-stock products",
                lowStock.Count);

            foreach (var product in lowStock)
            {
                try
                {
                    await notify
                        .SendStockAlertAsync(
                            product.Name,
                            product
                                .StockQuantity,
                            _threshold);

                    _log.LogWarning(
                        "Low stock: {Name}"
                        + " ({Qty} units)",
                        product.Name,
                        product
                            .StockQuantity);
                }
                catch (Exception ex)
                {
                    _log.LogError(
                        ex,
                        "Alert send failed"
                        + " for {Name}",
                        product.Name);
                }
            }
        }
    }
}
