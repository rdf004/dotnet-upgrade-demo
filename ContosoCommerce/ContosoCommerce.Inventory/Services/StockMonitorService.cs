using System;
using System.Data.Entity;
using System.Linq;
using System.Timers;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using log4net;

namespace ContosoCommerce.Inventory.Services
{
    /// <summary>
    /// Background stock monitoring using
    /// System.Timers.Timer. Runs on app start
    /// via Global.asax. In .NET 8 this should
    /// be replaced with IHostedService.
    /// </summary>
    public class StockMonitorService
        : IDisposable
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(StockMonitorService));

        private readonly Timer _timer;
        private readonly INotificationService
            _notifications;
        private readonly int _threshold;
        private readonly double _intervalMs;
        private bool _disposed;

        public StockMonitorService(
            INotificationService notifications,
            int lowStockThreshold = 10,
            double intervalMinutes = 30)
        {
            _notifications = notifications;
            _threshold = lowStockThreshold;
            _intervalMs =
                intervalMinutes * 60 * 1000;
            _timer = new Timer(_intervalMs);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
        }

        public void Start()
        {
            Log.Info(
                "Stock monitor starting. "
                + "Interval: "
                + (_intervalMs / 60000)
                + " min");
            _timer.Start();
            CheckStockLevels();
        }

        public void Stop()
        {
            Log.Info(
                "Stock monitor stopping.");
            _timer.Stop();
        }

        private void OnTimerElapsed(
            object sender, ElapsedEventArgs e)
        {
            try
            {
                CheckStockLevels();
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Stock check failed", ex);
            }
        }

        private void CheckStockLevels()
        {
            Log.Debug(
                "Running stock level check.");

            using (var db =
                new CommerceDbContext())
            {
                var lowStockProducts = db
                    .Products
                    .Where(p => p.IsActive
                        && p.StockQuantity
                            <= _threshold)
                    .ToList();

                Log.InfoFormat(
                    "Found {0} low-stock "
                    + "products",
                    lowStockProducts.Count);

                foreach (var product
                    in lowStockProducts)
                {
                    try
                    {
                        _notifications
                            .SendStockAlertAsync(
                                product.Name,
                                product
                                    .StockQuantity,
                                _threshold)
                            .Wait();

                        Log.WarnFormat(
                            "Low stock alert: "
                            + "{0} ({1} units)",
                            product.Name,
                            product
                                .StockQuantity);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(
                            "Alert send failed "
                            + "for "
                            + product.Name,
                            ex);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer.Dispose();
                _disposed = true;
            }
        }
    }
}
