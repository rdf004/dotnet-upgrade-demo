using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;
using System.IO;
using ContosoCommerce.Core.DTOs;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Users.Filters;
using log4net;

namespace ContosoCommerce.Reporting.Controllers
{
    /// <summary>
    /// Reporting and analytics endpoints.
    /// Includes a legacy ASMX-style XML endpoint
    /// for a "legacy integration partner".
    /// </summary>
    [TokenAuthorize]
    [RoutePrefix("api/reports")]
    public class ReportsController
        : ApiController
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(ReportsController));

        private readonly IReportService _svc;

        public ReportsController(
            IReportService reportService)
        {
            _svc = reportService;
        }

        /// <summary>
        /// GET api/reports/sales
        /// </summary>
        [HttpGet]
        [Route("sales")]
        public async Task<IHttpActionResult>
            GetSalesReport(
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var start = startDate
                ?? DateTime.UtcNow
                    .AddDays(-30);
            var end = endDate
                ?? DateTime.UtcNow;

            var report = await _svc
                .GetSalesReportAsync(
                    start, end);
            return Ok(
                ApiResponse<SalesReportDto>
                    .Ok(report));
        }

        /// <summary>
        /// GET api/reports/inventory
        /// </summary>
        [HttpGet]
        [Route("inventory")]
        public async Task<IHttpActionResult>
            GetInventoryReport()
        {
            var report = await _svc
                .GetInventoryReportAsync();
            return Ok(
                ApiResponse<
                    InventoryReportDto>
                    .Ok(report));
        }

        /// <summary>
        /// GET api/reports/users
        /// </summary>
        [HttpGet]
        [Route("users")]
        public async Task<IHttpActionResult>
            GetUserActivityReport(
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var start = startDate
                ?? DateTime.UtcNow
                    .AddDays(-30);
            var end = endDate
                ?? DateTime.UtcNow;

            var report = await _svc
                .GetUserActivityReportAsync(
                    start, end);
            return Ok(
                ApiResponse<
                    UserActivityReportDto>
                    .Ok(report));
        }

        /// <summary>
        /// GET api/reports/sales/chart
        /// </summary>
        [HttpGet]
        [Route("sales/chart")]
        public async Task<HttpResponseMessage>
            GetSalesChart(
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var start = startDate
                ?? DateTime.UtcNow
                    .AddDays(-30);
            var end = endDate
                ?? DateTime.UtcNow;

            var imageData = await _svc
                .GetSalesChartImageAsync(
                    start, end);

            var response =
                new HttpResponseMessage(
                    HttpStatusCode.OK);
            response.Content =
                new ByteArrayContent(
                    imageData);
            response.Content.Headers
                .ContentType =
                    new MediaTypeHeaderValue(
                        "image/png");
            return response;
        }

        /// <summary>
        /// GET api/reports/sales/csv
        /// </summary>
        [HttpGet]
        [Route("sales/csv")]
        public async Task<HttpResponseMessage>
            ExportSalesCsv(
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var start = startDate
                ?? DateTime.UtcNow
                    .AddDays(-30);
            var end = endDate
                ?? DateTime.UtcNow;

            var csv = await _svc
                .ExportSalesReportCsvAsync(
                    start, end);

            var response =
                new HttpResponseMessage(
                    HttpStatusCode.OK);
            response.Content =
                new StringContent(
                    csv,
                    Encoding.UTF8,
                    "text/csv");
            response.Content.Headers
                .ContentDisposition =
                    new ContentDispositionHeaderValue(
                        "attachment")
                    {
                        FileName =
                            "sales_report.csv"
                    };
            return response;
        }

        /// <summary>
        /// GET api/reports/inventory/csv
        /// </summary>
        [HttpGet]
        [Route("inventory/csv")]
        public async Task<HttpResponseMessage>
            ExportInventoryCsv()
        {
            var csv = await _svc
                .ExportInventoryReportCsvAsync();

            var response =
                new HttpResponseMessage(
                    HttpStatusCode.OK);
            response.Content =
                new StringContent(
                    csv,
                    Encoding.UTF8,
                    "text/csv");
            response.Content.Headers
                .ContentDisposition =
                    new ContentDispositionHeaderValue(
                        "attachment")
                    {
                        FileName =
                            "inventory_report"
                            + ".csv"
                    };
            return response;
        }

        /// <summary>
        /// GET api/reports/sales/xml
        /// Legacy ASMX-style XML endpoint for
        /// a "legacy integration partner".
        /// Returns XML via XmlSerializer.
        /// </summary>
        [HttpGet]
        [Route("sales/xml")]
        public async Task<HttpResponseMessage>
            GetSalesReportXml(
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var start = startDate
                ?? DateTime.UtcNow
                    .AddDays(-30);
            var end = endDate
                ?? DateTime.UtcNow;

            var report = await _svc
                .GetSalesReportAsync(
                    start, end);

            var serializer =
                new XmlSerializer(
                    typeof(SalesReportDto));

            using (var writer =
                new StringWriter())
            {
                serializer.Serialize(
                    writer, report);

                var response =
                    new HttpResponseMessage(
                        HttpStatusCode.OK);
                response.Content =
                    new StringContent(
                        writer.ToString(),
                        Encoding.UTF8,
                        "application/xml");
                return response;
            }
        }
    }
}
