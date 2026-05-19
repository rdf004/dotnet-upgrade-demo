using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCommerce.Core.Enums;

namespace ContosoCommerce.Core.Interfaces
{
    /// <summary>
    /// Manages products, categories, and stock.
    /// </summary>
    public interface IInventoryService
    {
        Task<ProductDto> GetProductAsync(
            int productId);

        Task<IList<ProductDto>> GetProductsAsync(
            int page, int pageSize,
            int? categoryId = null);

        Task<ProductDto> CreateProductAsync(
            CreateProductRequest request);

        Task<ProductDto> UpdateProductAsync(
            int productId,
            UpdateProductRequest request);

        Task DeleteProductAsync(int productId);

        Task<IList<CategoryDto>> GetCategoriesAsync();

        Task<CategoryDto> CreateCategoryAsync(
            CreateCategoryRequest request);

        Task<bool> ReserveStockAsync(
            int productId, int quantity);

        Task ReleaseStockAsync(
            int productId, int quantity);

        Task<StockLevel> GetStockLevelAsync(
            int productId);

        Task AdjustStockAsync(
            int productId,
            int newQuantity);

        Task<IList<ProductDto>>
            GetLowStockProductsAsync(
                int threshold);

        Task<int> GetProductCountAsync();

        Task<byte[]> GetProductImageAsync(
            int productId);
    }

    [System.Serializable]
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
        public StockLevel StockLevel { get; set; }
    }

    [System.Serializable]
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; }
        public int? CategoryId { get; set; }
        public byte[] ImageData { get; set; }
    }

    [System.Serializable]
    public class UpdateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Sku { get; set; }
        public int? CategoryId { get; set; }
        public byte[] ImageData { get; set; }
    }

    [System.Serializable]
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public int ProductCount { get; set; }
    }

    [System.Serializable]
    public class CreateCategoryRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
