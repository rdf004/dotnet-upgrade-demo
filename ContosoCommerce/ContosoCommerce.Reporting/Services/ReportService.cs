using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing
    .Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ContosoCommerce.Reporting.Services
{
    public class ReportService
        : IReportService
    {
        private readonly
            ILogger<ReportService> _log;
        private readonly CommerceDbContext _db;
        private readonly IMemoryCache _cache;

        private const int ChartWidth = 800;
        private const int ChartHeight = 400;
        private const int CacheMins = 15;

        public ReportService(
            CommerceDbContext context,
            IMemoryCache cache,
            ILogger<ReportService> logger)
        {
            _db = context;
            _cache = cache;
            _log = logger;
        }

        public async Task<SalesReportDto>
            GenerateSalesReportAsync(
                DateTime startDate,
                DateTime endDate)
        {
            var cacheKey = string.Format(
                "sales_{0}_{1}",
                startDate
                    .ToString("yyyyMMdd"),
                endDate
                    .ToString("yyyyMMdd"));

            if (_cache.TryGetValue(
                cacheKey,
                out SalesReportDto cached))
            {
                _log.LogDebug(
                    "Sales report from cache");
                return cached;
            }

            _log.LogInformation(
                "Generating sales report"
                + " {Start} to {End}",
                startDate, endDate);

            var orders = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(
                        i => i.Product)
                .Where(o =>
                    o.OrderDate >= startDate
                    && o.OrderDate <= endDate)
                .ToListAsync();

            var dailySales = orders
                .GroupBy(
                    o => o.OrderDate.Date)
                .Select(g =>
                    new DailySalesDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(
                            o => o.TotalAmount),
                        OrderCount =
                            g.Count()
                    })
                .OrderBy(d => d.Date)
                .ToList();

            var topProducts = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => new
                {
                    i.ProductId,
                    Name =
                        i.Product != null
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
                                i => i
                                    .Quantity),
                        Revenue =
                            g.Sum(
                                i => i
                                    .LineTotal)
                    })
                .OrderByDescending(
                    p => p.Revenue)
                .Take(10)
                .ToList();

            var totalRev =
                orders.Sum(
                    o => o.TotalAmount);
            var totalOrds = orders.Count;

            var report = new SalesReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRev,
                TotalOrders = totalOrds,
                AverageOrderValue =
                    totalOrds > 0
                        ? totalRev / totalOrds
                        : 0,
                DailySales = dailySales,
                TopProducts = topProducts
            };

            _cache.Set(
                cacheKey, report,
                TimeSpan.FromMinutes(
                    CacheMins));

            return report;
        }

        public async Task<InventoryReportDto>
            GenerateInventoryReportAsync()
        {
            var cacheKey =
                "inventory_report";

            if (_cache.TryGetValue(
                cacheKey,
                out InventoryReportDto c))
            {
                return c;
            }

            _log.LogInformation(
                "Generating inventory"
                + " report");

            var products = await _db.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            var summaries = products
                .Select(p =>
                    new StockSummaryDto
                    {
                        ProductId = p.Id,
                        ProductName =
                            p.Name,
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
                            p => p
                                .StockQuantity
                                <= 0),
                    LowStockCount =
                        products.Count(
                            p => p
                                .StockQuantity
                                > 0
                                && p
                                    .StockQuantity
                                    <= 10),
                    TotalInventoryValue =
                        summaries.Sum(
                            s => s
                                .TotalValue),
                    StockSummaries =
                        summaries
                };

            _cache.Set(
                cacheKey, report,
                TimeSpan.FromMinutes(
                    CacheMins));

            return report;
        }

        public async
            Task<UserActivityReportDto>
            GetUserActivityReportAsync(
                DateTime startDate,
                DateTime endDate)
        {
            _log.LogInformation(
                "Generating user activity"
                + " report {Start} to {End}",
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
                    var uo = orders
                        .Where(
                            o => o.UserId
                                == u.Id)
                        .ToList();
                    return
                        new UserActivityDto
                        {
                            UserId = u.Id,
                            UserEmail =
                                u.Email,
                            OrderCount =
                                uo.Count,
                            TotalSpent =
                                uo.Sum(o =>
                                    o.TotalAmount),
                            LastActiveDate =
                                uo.Any()
                                    ? uo.Max(
                                        o => o
                                            .OrderDate)
                                    : u
                                        .CreatedAt
                        };
                })
                .OrderByDescending(
                    a => a.TotalSpent)
                .ToList();

            var newRegs = users.Count(
                u => u.CreatedAt >= startDate
                    && u.CreatedAt
                        <= endDate);

            return new UserActivityReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalUsers = users.Count,
                ActiveUsers = activities
                    .Count(
                        a => a.OrderCount > 0),
                NewRegistrations = newRegs,
                Activities = activities
            };
        }

        public async Task<byte[]>
            GetSalesChartImageAsync(
                DateTime startDate,
                DateTime endDate)
        {
            var report =
                await
                    GenerateSalesReportAsync(
                        startDate, endDate);

            try
            {
                using var img =
                    new Image<Rgba32>(
                        ChartWidth,
                        ChartHeight);
                img.Mutate(g =>
                {
                    g.Fill(
                        SixLabors.ImageSharp
                            .Color.White);
                });

                using var ms =
                    new MemoryStream();
                img.SaveAsPng(ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _log.LogError(
                    ex,
                    "Chart generation"
                    + " failed");
                return Array.Empty<byte>();
            }
        }

        public async Task<string>
            ExportSalesReportCsvAsync(
                DateTime startDate,
                DateTime endDate)
        {
            var report =
                await
                    GenerateSalesReportAsync(
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
                await
                    GenerateInventoryReportAsync();

            var exporter =
                new ExportService();
            return exporter
                .ExportInventoryReportToCsv(
                    report);
        }

        public async
            Task<DashboardSummaryDto>
            GetDashboardSummaryAsync()
        {
            var users = await _db.Users
                .CountAsync(u => u.IsActive);
            var products = await _db.Products
                .CountAsync(p => p.IsActive);
            var orders = await _db.Orders
                .CountAsync();
            var revenue = await _db.Orders
                .Where(o =>
                    o.Status
                    != Core.Enums
                        .OrderStatus.Cancelled
                    && o.Status
                    != Core.Enums
                        .OrderStatus.Refunded)
                .SumAsync(o => o.TotalAmount);
            return new DashboardSummaryDto
            {
                TotalUsers = users,
                TotalProducts = products,
                TotalOrders = orders,
                TotalRevenue = revenue
            };
        }

        private static string
            GetStockLabel(int quantity)
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
