using System;
using System.Threading;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using log4net;

namespace ContosoCommerce.Reporting.Services
{
    /// <summary>
    /// Pre-generates daily reports using
    /// System.Threading.Timer. In .NET 8 this
    /// should be replaced with IHostedService
    /// and BackgroundService.
    /// </summary>
    public class ReportScheduler : IDisposable
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(ReportScheduler));

        private Timer _timer;
        private bool _disposed;
        private readonly TimeSpan _interval;

        public ReportScheduler(
            TimeSpan? interval = null)
        {
            _interval = interval
                ?? TimeSpan.FromHours(1);
        }

        public void Start()
        {
            Log.InfoFormat(
                "Report scheduler starting. "
                + "Interval: {0}",
                _interval);

            _timer = new Timer(
                GenerateReports,
                null,
                TimeSpan.Zero,
                _interval);
        }

        public void Stop()
        {
            Log.Info(
                "Report scheduler stopping.");
            _timer?.Change(
                Timeout.Infinite,
                Timeout.Infinite);
        }

        private void GenerateReports(
            object state)
        {
            try
            {
                Log.Info(
                    "Scheduled report "
                    + "generation starting.");

                using (var db =
                    new CommerceDbContext())
                {
                    var svc =
                        new ReportService(db);

                    var endDate =
                        DateTime.UtcNow;
                    var startDate =
                        endDate.AddDays(-30);

                    svc.GetSalesReportAsync(
                        startDate, endDate)
                        .Wait();

                    svc
                        .GetInventoryReportAsync()
                        .Wait();

                    svc
                        .GetUserActivityReportAsync(
                            startDate, endDate)
                        .Wait();
                }

                Log.Info(
                    "Scheduled reports "
                    + "generated and cached.");
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Scheduled report "
                    + "generation failed",
                    ex);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Dispose();
                _disposed = true;
            }
        }
    }
}
