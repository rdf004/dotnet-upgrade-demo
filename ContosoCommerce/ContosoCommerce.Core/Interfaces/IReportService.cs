using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContosoCommerce.Core.Interfaces
{
    public interface IReportService
    {
        Task<SalesReportDto>
            GenerateSalesReportAsync(
                DateTime startDate,
                DateTime endDate);

        Task<InventoryReportDto>
            GenerateInventoryReportAsync();

        Task<UserActivityReportDto>
            GetUserActivityReportAsync(
                DateTime startDate,
                DateTime endDate);

        Task<byte[]>
            GetSalesChartImageAsync(
                DateTime startDate,
                DateTime endDate);

        Task<string>
            ExportSalesReportCsvAsync(
                DateTime startDate,
                DateTime endDate);

        Task<string>
            ExportInventoryReportCsvAsync();

        Task<DashboardSummaryDto>
            GetDashboardSummaryAsync();
    }

    [Serializable]
    public class SalesReportDto
    {
        public DateTime StartDate
        {
            get; set;
        }
        public DateTime EndDate
        {
            get; set;
        }
        public decimal TotalRevenue
        {
            get; set;
        }
        public int TotalOrders
        {
            get; set;
        }
        public decimal AverageOrderValue
        {
            get; set;
        }

        public IList<DailySalesDto>
            DailySales
        {
            get; set;
        }

        public IList<TopProductDto>
            TopProducts
        {
            get; set;
        }
    }

    [Serializable]
    public class DailySalesDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    [Serializable]
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName
        {
            get; set;
        }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    [Serializable]
    public class InventoryReportDto
    {
        public int TotalProducts
        {
            get; set;
        }
        public int OutOfStockCount
        {
            get; set;
        }
        public int LowStockCount
        {
            get; set;
        }
        public decimal TotalInventoryValue
        {
            get; set;
        }

        public IList<StockSummaryDto>
            StockSummaries
        {
            get; set;
        }
    }

    [Serializable]
    public class StockSummaryDto
    {
        public int ProductId { get; set; }
        public string ProductName
        {
            get; set;
        }
        public int Quantity { get; set; }
        public decimal UnitPrice
        {
            get; set;
        }
        public decimal TotalValue
        {
            get; set;
        }
        public string StockLevel
        {
            get; set;
        }
    }

    [Serializable]
    public class UserActivityReportDto
    {
        public DateTime StartDate
        {
            get; set;
        }
        public DateTime EndDate
        {
            get; set;
        }
        public int TotalUsers { get; set; }
        public int ActiveUsers
        {
            get; set;
        }
        public int NewRegistrations
        {
            get; set;
        }

        public IList<UserActivityDto>
            Activities
        {
            get; set;
        }
    }

    [Serializable]
    public class UserActivityDto
    {
        public int UserId { get; set; }
        public string UserEmail
        {
            get; set;
        }
        public int OrderCount { get; set; }
        public decimal TotalSpent
        {
            get; set;
        }
        public DateTime LastActiveDate
        {
            get; set;
        }
    }

    [Serializable]
    public class DashboardSummaryDto
    {
        public int TotalUsers { get; set; }
        public int TotalProducts
        {
            get; set;
        }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue
        {
            get; set;
        }
    }
}
