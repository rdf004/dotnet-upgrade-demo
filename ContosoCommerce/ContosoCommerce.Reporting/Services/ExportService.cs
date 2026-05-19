using System.Text;
using ContosoCommerce.Core.Interfaces;

namespace ContosoCommerce.Reporting.Services
{
    /// <summary>
    /// Generates CSV exports using manual
    /// StringBuilder (no external library).
    /// </summary>
    public class ExportService
    {
        public string ExportSalesReportToCsv(
            SalesReportDto report)
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                "Date,Revenue,OrderCount");

            if (report.DailySales != null)
            {
                foreach (var day
                    in report.DailySales)
                {
                    sb.AppendFormat(
                        "{0},{1:F2},{2}",
                        day.Date
                            .ToString(
                                "yyyy-MM-dd"),
                        day.Revenue,
                        day.OrderCount);
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            sb.AppendLine("Top Products");
            sb.AppendLine(
                "ProductId,Name,"
                + "QtySold,Revenue");

            if (report.TopProducts != null)
            {
                foreach (var p
                    in report.TopProducts)
                {
                    sb.AppendFormat(
                        "{0},\"{1}\",{2},"
                        + "{3:F2}",
                        p.ProductId,
                        EscapeCsv(
                            p.ProductName),
                        p.QuantitySold,
                        p.Revenue);
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            sb.AppendFormat(
                "Total Revenue,{0:F2}",
                report.TotalRevenue);
            sb.AppendLine();
            sb.AppendFormat(
                "Total Orders,{0}",
                report.TotalOrders);
            sb.AppendLine();
            sb.AppendFormat(
                "Avg Order Value,{0:F2}",
                report.AverageOrderValue);
            sb.AppendLine();

            return sb.ToString();
        }

        public string
            ExportInventoryReportToCsv(
                InventoryReportDto report)
        {
            var sb = new StringBuilder();

            sb.AppendLine(
                "ProductId,Name,Qty,"
                + "UnitPrice,TotalValue,"
                + "StockLevel");

            if (report.StockSummaries != null)
            {
                foreach (var s
                    in report.StockSummaries)
                {
                    sb.AppendFormat(
                        "{0},\"{1}\",{2},"
                        + "{3:F2},{4:F2},{5}",
                        s.ProductId,
                        EscapeCsv(
                            s.ProductName),
                        s.Quantity,
                        s.UnitPrice,
                        s.TotalValue,
                        s.StockLevel);
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            sb.AppendFormat(
                "Total Products,{0}",
                report.TotalProducts);
            sb.AppendLine();
            sb.AppendFormat(
                "Out of Stock,{0}",
                report.OutOfStockCount);
            sb.AppendLine();
            sb.AppendFormat(
                "Low Stock,{0}",
                report.LowStockCount);
            sb.AppendLine();
            sb.AppendFormat(
                "Total Inventory Value,"
                + "{0:F2}",
                report
                    .TotalInventoryValue);
            sb.AppendLine();

            return sb.ToString();
        }

        private static string EscapeCsv(
            string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Replace(
                "\"", "\"\"");
        }
    }
}
