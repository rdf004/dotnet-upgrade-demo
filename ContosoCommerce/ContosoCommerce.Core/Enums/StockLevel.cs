namespace ContosoCommerce.Core.Enums
{
    /// <summary>
    /// Thresholds for inventory stock alerts.
    /// </summary>
    public enum StockLevel
    {
        OutOfStock = 0,
        Critical = 1,
        Low = 2,
        Normal = 3,
        Overstocked = 4
    }
}
