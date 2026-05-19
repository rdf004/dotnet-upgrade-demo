using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using log4net;

namespace ContosoCommerce.Inventory.Services
{
    /// <summary>
    /// Manages products and stock levels. Uses
    /// System.Drawing for image thumbnails (a
    /// platform-specific API that must be replaced
    /// with ImageSharp in .NET 8). Accesses
    /// HttpContext.Current for request context.
    /// </summary>
    public class InventoryService
        : IInventoryService
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(InventoryService));

        private readonly CommerceDbContext _db;
        private readonly IAuditService _audit;
        private readonly IUserService _users;

        private const int ThumbWidth = 150;
        private const int ThumbHeight = 150;
        private const int LowStockThreshold = 10;

        public InventoryService(
            CommerceDbContext context,
            IAuditService auditService,
            IUserService userService)
        {
            _db = context;
            _audit = auditService;
            _users = userService;
        }

        public async Task<ProductDto>
            GetProductAsync(int productId)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(
                    p => p.Id == productId);

            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }
            return MapToDto(product);
        }

        public async Task<IList<ProductDto>>
            GetProductsAsync(
                int page, int pageSize,
                int? categoryId = null)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
            {
                query = query.Where(
                    p => p.CategoryId
                        == categoryId.Value);
            }

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products
                .Select(MapToDto)
                .ToList();
        }

        public async Task<ProductDto>
            CreateProductAsync(
                CreateProductRequest request)
        {
            ValidateManagerRole();

            Log.InfoFormat(
                "Creating product: {0}",
                request.Name);

            var product = new Product
            {
                Name = request.Name,
                Description =
                    request.Description,
                Price = request.Price,
                StockQuantity =
                    request.StockQuantity,
                Sku = request.Sku,
                CategoryId =
                    request.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            if (request.ImageData != null
                && request.ImageData.Length > 0)
            {
                product.ImageData =
                    request.ImageData;
                product.ThumbnailData =
                    GenerateThumbnail(
                        request.ImageData);
            }

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                "Product", product.Id,
                "Create",
                string.Format(
                    "Product {0} created",
                    product.Name));

            return MapToDto(product);
        }

        public async Task<ProductDto>
            UpdateProductAsync(
                int productId,
                UpdateProductRequest request)
        {
            ValidateManagerRole();

            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }

            if (!string.IsNullOrEmpty(
                request.Name))
            {
                product.Name = request.Name;
            }
            if (request.Description != null)
            {
                product.Description =
                    request.Description;
            }
            if (request.Price > 0)
            {
                product.Price = request.Price;
            }
            if (!string.IsNullOrEmpty(
                request.Sku))
            {
                product.Sku = request.Sku;
            }
            product.CategoryId =
                request.CategoryId;

            if (request.ImageData != null
                && request.ImageData.Length > 0)
            {
                product.ImageData =
                    request.ImageData;
                product.ThumbnailData =
                    GenerateThumbnail(
                        request.ImageData);
            }

            product.UpdatedAt =
                DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                "Product", product.Id,
                "Update",
                string.Format(
                    "Product {0} updated",
                    product.Name));

            return MapToDto(product);
        }

        public async Task DeleteProductAsync(
            int productId)
        {
            ValidateManagerRole();

            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }

            product.IsActive = false;
            product.UpdatedAt =
                DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                "Product", product.Id,
                "Delete",
                string.Format(
                    "Product {0} deactivated",
                    product.Name));
        }

        public async Task<IList<CategoryDto>>
            GetCategoriesAsync()
        {
            var categories = await _db
                .Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Products)
                .ToListAsync();

            return categories.Select(c =>
                new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description =
                        c.Description,
                    ParentCategoryId =
                        c.ParentCategoryId,
                    ParentCategoryName =
                        c.ParentCategory != null
                            ? c.ParentCategory
                                .Name
                            : null,
                    ProductCount =
                        c.Products.Count
                }).ToList();
        }

        public async Task<CategoryDto>
            CreateCategoryAsync(
                CreateCategoryRequest request)
        {
            ValidateManagerRole();

            var category = new Category
            {
                Name = request.Name,
                Description =
                    request.Description,
                ParentCategoryId =
                    request.ParentCategoryId
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description =
                    category.Description,
                ParentCategoryId =
                    category.ParentCategoryId,
                ProductCount = 0
            };
        }

        public async Task<bool>
            ReserveStockAsync(
                int productId, int quantity)
        {
            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }

            if (product.StockQuantity
                < quantity)
            {
                Log.WarnFormat(
                    "Insufficient stock for "
                    + "product {0}: have {1}, "
                    + "need {2}",
                    productId,
                    product.StockQuantity,
                    quantity);
                return false;
            }

            product.StockQuantity -= quantity;
            await _db.SaveChangesAsync();

            Log.InfoFormat(
                "Reserved {0} of product {1}",
                quantity, productId);
            return true;
        }

        public async Task ReleaseStockAsync(
            int productId, int quantity)
        {
            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }

            product.StockQuantity += quantity;
            await _db.SaveChangesAsync();
        }

        public async Task<StockLevel>
            GetStockLevelAsync(int productId)
        {
            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }
            return ClassifyStockLevel(
                product.StockQuantity);
        }

        public async Task AdjustStockAsync(
            int productId, int newQuantity)
        {
            ValidateManagerRole();

            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }

            var oldQty =
                product.StockQuantity;
            product.StockQuantity =
                newQuantity;
            await _db.SaveChangesAsync();

            await _audit.LogAsync(
                "Product", productId,
                "StockAdjust",
                string.Format(
                    "Stock changed from {0} "
                    + "to {1}",
                    oldQty, newQuantity));
        }

        public async Task<IList<ProductDto>>
            GetLowStockProductsAsync(
                int threshold)
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive
                    && p.StockQuantity
                        <= threshold)
                .OrderBy(
                    p => p.StockQuantity)
                .ToListAsync();

            return products
                .Select(MapToDto)
                .ToList();
        }

        public async Task<int>
            GetProductCountAsync()
        {
            return await _db.Products
                .CountAsync(p => p.IsActive);
        }

        public async Task<byte[]>
            GetProductImageAsync(
                int productId)
        {
            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(
                    "Product", productId);
            }
            return product.ThumbnailData
                ?? product.ImageData;
        }

        /// <summary>
        /// Generates a thumbnail using
        /// System.Drawing (Windows-only API).
        /// </summary>
        private byte[] GenerateThumbnail(
            byte[] imageData)
        {
            try
            {
                using (var ms =
                    new MemoryStream(imageData))
                using (var original =
                    Image.FromStream(ms))
                using (var thumb =
                    original
                        .GetThumbnailImage(
                            ThumbWidth,
                            ThumbHeight,
                            null,
                            IntPtr.Zero))
                using (var output =
                    new MemoryStream())
                {
                    thumb.Save(
                        output,
                        ImageFormat.Png);
                    return output.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Thumbnail generation "
                    + "failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Validates the requesting user has the
        /// InventoryManager role by reading
        /// HttpContext.Current.
        /// </summary>
        private void ValidateManagerRole()
        {
            var ctx = HttpContext.Current;
            if (ctx == null) return;

            var userIdObj =
                ctx.Items["UserId"];
            if (userIdObj == null) return;

            var userId = (int)userIdObj;
            var hasRole = _users
                .HasRoleAsync(
                    userId,
                    UserRole.InventoryManager)
                .Result;

            if (!hasRole)
            {
                throw new BusinessRuleException(
                    "InsufficientRole",
                    "User does not have the "
                    + "InventoryManager role.");
            }
        }

        private static StockLevel
            ClassifyStockLevel(int quantity)
        {
            if (quantity <= 0)
                return StockLevel.OutOfStock;
            if (quantity <= 5)
                return StockLevel.Critical;
            if (quantity <= 10)
                return StockLevel.Low;
            if (quantity <= 500)
                return StockLevel.Normal;
            return StockLevel.Overstocked;
        }

        private static ProductDto MapToDto(
            Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity =
                    p.StockQuantity,
                Sku = p.Sku,
                CategoryId = p.CategoryId,
                CategoryName =
                    p.Category != null
                        ? p.Category.Name
                        : null,
                IsActive = p.IsActive,
                StockLevel =
                    ClassifyStockLevel(
                        p.StockQuantity)
            };
        }
    }
}
