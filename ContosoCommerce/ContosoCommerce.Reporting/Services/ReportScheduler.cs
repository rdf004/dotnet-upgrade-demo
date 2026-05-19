using System;
using System.Threading;
using System.Threading.Tasks;
using ContosoCommerce.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Reporting.Services
{
    public class ReportSchedulerWorker
        : BackgroundService
    {
        private readonly
            ILogger<ReportSchedulerWorker>
            _log;
        private readonly
            IServiceScopeFactory
            _scopeFactory;
        private readonly TimeSpan _interval;

        public ReportSchedulerWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ReportSchedulerWorker>
                logger,
            TimeSpan? interval = null)
        {
            _scopeFactory = scopeFactory;
            _log = logger;
            _interval = interval
                ?? TimeSpan.FromHours(1);
        }

        protected override async Task
            ExecuteAsync(
                CancellationToken
                    stoppingToken)
        {
            _log.LogInformation(
                "Report scheduler starting."
                + " Interval: {Interval}",
                _interval);

            while (!stoppingToken
                .IsCancellationRequested)
            {
                try
                {
                    await GenerateReports();
                }
                catch (Exception ex)
                {
                    _log.LogError(
                        ex,
                        "Scheduled report"
                        + " generation failed");
                }

                await Task.Delay(
                    _interval,
                    stoppingToken);
            }

            _log.LogInformation(
                "Report scheduler"
                + " stopping.");
        }

        private async Task
            GenerateReports()
        {
            _log.LogInformation(
                "Scheduled report"
                + " generation starting.");

            using var scope =
                _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider
                .GetRequiredService<
                    IReportService>();

            var endDate = DateTime.UtcNow;
            var startDate =
                endDate.AddDays(-30);

            await svc
                .GenerateSalesReportAsync(
                    startDate, endDate);
            await svc
                .GenerateInventoryReportAsync();
            await svc
                .GetUserActivityReportAsync(
                    startDate, endDate);

            _log.LogInformation(
                "Scheduled reports"
                + " generated and cached.");
        }
    }
}
