using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using log4net;

namespace ContosoCommerce.Reporting.Services
{
    /// <summary>
    /// Generates reports by querying across all
    /// modules via the shared DbContext. Uses
    /// System.Drawing for chart images,
    /// MemoryCache and HttpRuntime.Cache for
    /// caching (both need replacement in .NET 8).
    /// </summary>
    public class ReportService
        : IReportService
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(ReportService));

        private readonly CommerceDbContext _db;
        private static readonly MemoryCache
            Cache = MemoryCache.Default;

        private const int ChartWidth = 800;
        private const int ChartHeight = 400;
        private const int CacheMins = 15;

        public ReportService(
            CommerceDbContext context)
        {
            _db = context;
        }

        public async Task<SalesReportDto>
            GetSalesReportAsync(
                DateTime startDate,
                DateTime endDate)
        {
            var cacheKey = string.Format(
                "sales_{0}_{1}",
                startDate.ToString("yyyyMMdd"),
                endDate.ToString("yyyyMMdd"));

            var cached = Cache.Get(cacheKey)
                as SalesReportDto;
            if (cached != null)
            {
                Log.Debug(
                    "Sales report from cache");
                return cached;
            }

            Log.InfoFormat(
                "Generating sales report "
                + "{0} to {1}",
                startDate, endDate);

            var orders = await _db.Orders
                .Include(o => o.Items
                    .Select(i => i.Product))
                .Where(o =>
                    o.OrderDate >= startDate
                    && o.OrderDate <= endDate)
                .ToListAsync();

            var dailySales = orders
                .GroupBy(o =>
                    o.OrderDate.Date)
                .Select(g =>
                    new DailySalesDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(
                            o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                .OrderBy(d => d.Date)
                .ToList();

            var topProducts = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => new
                {
                    i.ProductId,
                    Name = i.Product != null
                        ? i.Product.Name
                        : "Unknown"
                })
                .Select(g =>
                    new TopProductDto
                    {
                        ProductId =
                            g.Key.ProductId,
                        ProductName =
                            g.Key.Name,
                        QuantitySold =
                            g.Sum(
                                i => i.Quantity),
                        Revenue =
                            g.Sum(
                                i => i.LineTotal)
                    })
                .OrderByDescending(
                    p => p.Revenue)
                .Take(10)
                .ToList();

            var totalRevenue =
                orders.Sum(
                    o => o.TotalAmount);
            var totalOrders = orders.Count;

            var report = new SalesReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue =
                    totalOrders > 0
                        ? totalRevenue
                            / totalOrders
                        : 0,
                DailySales = dailySales,
                TopProducts = topProducts
            };

            Cache.Set(
                cacheKey, report,
                new CacheItemPolicy
                {
                    AbsoluteExpiration =
                        DateTimeOffset.UtcNow
                            .AddMinutes(
                                CacheMins)
                });

            HttpRuntime.Cache.Insert(
                "last_sales_report",
                report,
                null,
                System.Web.Caching.Cache
                    .NoAbsoluteExpiration,
                TimeSpan.FromMinutes(
                    CacheMins));

            return report;
        }

        public async Task<InventoryReportDto>
            GetInventoryReportAsync()
        {
            var cacheKey = "inventory_report";
            var cached = Cache.Get(cacheKey)
                as InventoryReportDto;
            if (cached != null) return cached;

            Log.Info(
                "Generating inventory report");

            var products = await _db.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            var summaries = products
                .Select(p =>
                    new StockSummaryDto
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Quantity =
                            p.StockQuantity,
                        UnitPrice = p.Price,
                        TotalValue =
                            p.Price
                            * p.StockQuantity,
                        StockLevel =
                            GetStockLabel(
                                p.StockQuantity)
                    })
                .ToList();

            var report =
                new InventoryReportDto
                {
                    TotalProducts =
                        products.Count,
                    OutOfStockCount =
                        products.Count(
                            p => p.StockQuantity
                                <= 0),
                    LowStockCount =
                        products.Count(
                            p => p.StockQuantity
                                > 0
                                && p.StockQuantity
                                    <= 10),
                    TotalInventoryValue =
                        summaries.Sum(
                            s => s.TotalValue),
                    StockSummaries = summaries
                };

            Cache.Set(
                cacheKey, report,
                new CacheItemPolicy
                {
                    AbsoluteExpiration =
                        DateTimeOffset.UtcNow
                            .AddMinutes(
                                CacheMins)
                });

            return report;
        }

        public async Task<UserActivityReportDto>
            GetUserActivityReportAsync(
                DateTime startDate,
                DateTime endDate)
        {
            Log.InfoFormat(
                "Generating user activity "
                + "report {0} to {1}",
                startDate, endDate);

            var users = await _db.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var orders = await _db.Orders
                .Where(o =>
                    o.OrderDate >= startDate
                    && o.OrderDate <= endDate)
                .ToListAsync();

            var activities = users
                .Select(u =>
                {
                    var userOrders = orders
                        .Where(o =>
                            o.UserId == u.Id)
                        .ToList();
                    return new UserActivityDto
                    {
                        UserId = u.Id,
                        UserEmail = u.Email,
                        OrderCount =
                            userOrders.Count,
                        TotalSpent =
                            userOrders.Sum(
                                o => o
                                    .TotalAmount),
                        LastActiveDate =
                            userOrders.Any()
                                ? userOrders
                                    .Max(o =>
                                        o.OrderDate)
                                : u.CreatedAt
                    };
                })
                .OrderByDescending(
                    a => a.TotalSpent)
                .ToList();

            var newRegs = users.Count(
                u => u.CreatedAt >= startDate
                    && u.CreatedAt <= endDate);

            return new UserActivityReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalUsers = users.Count,
                ActiveUsers = activities
                    .Count(a =>
                        a.OrderCount > 0),
                NewRegistrations = newRegs,
                Activities = activities
            };
        }

        /// <summary>
        /// Generates a sales chart image using
        /// System.Drawing (Windows-only API).
        /// </summary>
        public async Task<byte[]>
            GetSalesChartImageAsync(
                DateTime startDate,
                DateTime endDate)
        {
            var report =
                await GetSalesReportAsync(
                    startDate, endDate);

            try
            {
                using (var bmp = new Bitmap(
                    ChartWidth, ChartHeight))
                using (var g =
                    Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);

                    g.DrawString(
                        "Sales Report",
                        new Font(
                            "Arial", 16,
                            FontStyle.Bold),
                        Brushes.Black,
                        new PointF(10, 10));

                    g.DrawString(
                        string.Format(
                            "Revenue: ${0:N2}",
                            report
                                .TotalRevenue),
                        new Font("Arial", 12),
                        Brushes.DarkGreen,
                        new PointF(10, 40));

                    g.DrawString(
                        string.Format(
                            "Orders: {0}",
                            report.TotalOrders),
                        new Font("Arial", 12),
                        Brushes.DarkBlue,
                        new PointF(10, 65));

                    var barX = 50;
                    var barY = 120;
                    var barWidth = 40;
                    var maxHeight = 250;

                    if (report.DailySales
                        .Count > 0)
                    {
                        var maxRev = report
                            .DailySales
                            .Max(d =>
                                d.Revenue);

                        foreach (var day
                            in report
                                .DailySales)
                        {
                            var h = maxRev > 0
                                ? (int)(
                                    (double)
                                        day.Revenue
                                    / (double)
                                        maxRev
                                    * maxHeight)
                                : 0;

                            g.FillRectangle(
                                Brushes
                                    .SteelBlue,
                                barX,
                                barY
                                    + maxHeight
                                    - h,
                                barWidth,
                                h);

                            g.DrawString(
                                day.Date
                                    .ToString(
                                        "MM/dd"),
                                new Font(
                                    "Arial", 8),
                                Brushes.Black,
                                new PointF(
                                    barX,
                                    barY
                                        + maxHeight
                                        + 5));

                            barX +=
                                barWidth + 10;
                        }
                    }

                    using (var ms =
                        new MemoryStream())
                    {
                        bmp.Save(
                            ms,
                            ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Chart generation failed",
                    ex);
                return new byte[0];
            }
        }

        public async Task<string>
            ExportSalesReportCsvAsync(
                DateTime startDate,
                DateTime endDate)
        {
            var report =
                await GetSalesReportAsync(
                    startDate, endDate);

            var exporter =
                new ExportService();
            return exporter
                .ExportSalesReportToCsv(
                    report);
        }

        public async Task<string>
            ExportInventoryReportCsvAsync()
        {
            var report =
                await GetInventoryReportAsync();

            var exporter =
                new ExportService();
            return exporter
                .ExportInventoryReportToCsv(
                    report);
        }

        private static string GetStockLabel(
            int quantity)
        {
            if (quantity <= 0)
                return "Out of Stock";
            if (quantity <= 5)
                return "Critical";
            if (quantity <= 10)
                return "Low";
            if (quantity <= 500)
                return "Normal";
            return "Overstocked";
        }
    }
}
