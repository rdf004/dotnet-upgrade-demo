using System;
using System.Threading.Tasks;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Reporting.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/reports")]
    public class ReportsController
        : ControllerBase
    {
        private readonly
            ILogger<ReportsController> _log;
        private readonly IReportService _svc;

        public ReportsController(
            IReportService reportService,
            ILogger<ReportsController> logger)
        {
            _svc = reportService;
            _log = logger;
        }

        [HttpGet("sales")]
        public async Task<IActionResult>
            SalesReport(
                DateTime? from = null,
                DateTime? to = null)
        {
            var startDate = from
                ?? DateTime.UtcNow
                    .AddMonths(-1);
            var endDate = to
                ?? DateTime.UtcNow;

            var report = await _svc
                .GenerateSalesReportAsync(
                    startDate, endDate);
            return Ok(
                ApiResponse<SalesReportDto>
                    .Ok(report));
        }

        [HttpGet("inventory")]
        public async Task<IActionResult>
            InventoryReport()
        {
            var report = await _svc
                .GenerateInventoryReportAsync();
            return Ok(
                ApiResponse<
                    InventoryReportDto>
                    .Ok(report));
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult>
            Dashboard()
        {
            var summary = await _svc
                .GetDashboardSummaryAsync();
            return Ok(
                ApiResponse<
                    DashboardSummaryDto>
                    .Ok(summary));
        }
    }
}
