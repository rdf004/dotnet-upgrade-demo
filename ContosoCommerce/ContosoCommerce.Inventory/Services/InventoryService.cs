using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Core.Exceptions;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ContosoCommerce.Inventory.Services
{
    public class InventoryService
        : IInventoryService
    {
        private readonly
            ILogger<InventoryService> _log;
        private readonly CommerceDbContext _db;
        private readonly IAuditService _audit;
        private readonly IUserService _users;
        private readonly
            IHttpContextAccessor _httpCtx;

        private const int ThumbWidth = 150;
        private const int ThumbHeight = 150;

        public InventoryService(
            CommerceDbContext context,
            IAuditService auditService,
            IUserService userService,
            IHttpContextAccessor httpCtx,
            ILogger<InventoryService> log)
        {
            _db = context;
            _audit = auditService;
            _users = userService;
            _httpCtx = httpCtx;
            _log = log;
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
                throw
                    new EntityNotFoundException(
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
                .Skip(
                    (page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products
                .Select(MapToDto)
                .ToList();
        }

        public async Task<ProductDto>
            CreateProductAsync(
                CreateProductRequest req)
        {
            ValidateManagerRole();

            _log.LogInformation(
                "Creating product: {Name}",
                req.Name);

            var product = new Product
            {
                Name = req.Name,
                Description =
                    req.Description,
                Price = req.Price,
                StockQuantity =
                    req.StockQuantity,
                Sku = req.Sku,
                CategoryId =
                    req.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            if (req.ImageData != null
                && req.ImageData.Length > 0)
            {
                product.ImageData =
                    req.ImageData;
                product.ThumbnailData =
                    GenerateThumbnail(
                        req.ImageData);
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
                UpdateProductRequest req)
        {
            ValidateManagerRole();

            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw
                    new EntityNotFoundException(
                        "Product", productId);
            }

            if (!string.IsNullOrEmpty(
                req.Name))
            {
                product.Name = req.Name;
            }
            if (req.Description != null)
            {
                product.Description =
                    req.Description;
            }
            if (req.Price > 0)
            {
                product.Price = req.Price;
            }
            if (!string.IsNullOrEmpty(
                req.Sku))
            {
                product.Sku = req.Sku;
            }
            product.CategoryId =
                req.CategoryId;

            if (req.ImageData != null
                && req.ImageData.Length > 0)
            {
                product.ImageData =
                    req.ImageData;
                product.ThumbnailData =
                    GenerateThumbnail(
                        req.ImageData);
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
                throw
                    new EntityNotFoundException(
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
                .Include(
                    c => c.ParentCategory)
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
                        c.ParentCategory
                            != null
                            ? c.ParentCategory
                                .Name
                            : null,
                    ProductCount =
                        c.Products.Count
                }).ToList();
        }

        public async Task<CategoryDto>
            CreateCategoryAsync(
                CreateCategoryRequest req)
        {
            ValidateManagerRole();

            var cat = new Category
            {
                Name = req.Name,
                Description =
                    req.Description,
                ParentCategoryId =
                    req.ParentCategoryId
            };

            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();

            return new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Description =
                    cat.Description,
                ParentCategoryId =
                    cat.ParentCategoryId,
                ProductCount = 0
            };
        }

        public async Task<bool>
            ReserveStockAsync(
                int productId,
                int quantity)
        {
            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw
                    new EntityNotFoundException(
                        "Product", productId);
            }

            if (product.StockQuantity
                < quantity)
            {
                _log.LogWarning(
                    "Insufficient stock"
                    + " for {Id}:"
                    + " have {Have},"
                    + " need {Need}",
                    productId,
                    product.StockQuantity,
                    quantity);
                return false;
            }

            product.StockQuantity
                -= quantity;
            await _db.SaveChangesAsync();

            _log.LogInformation(
                "Reserved {Qty}"
                + " of product {Id}",
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
                throw
                    new EntityNotFoundException(
                        "Product", productId);
            }

            product.StockQuantity
                += quantity;
            await _db.SaveChangesAsync();
        }

        public async Task<StockLevel>
            GetStockLevelAsync(
                int productId)
        {
            var product = await _db.Products
                .FindAsync(productId);
            if (product == null)
            {
                throw
                    new EntityNotFoundException(
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
                throw
                    new EntityNotFoundException(
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
                    "Stock changed"
                    + " from {0} to {1}",
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
                throw
                    new EntityNotFoundException(
                        "Product", productId);
            }
            return product.ThumbnailData
                ?? product.ImageData;
        }

        private byte[] GenerateThumbnail(
            byte[] imageData)
        {
            try
            {
                using var img =
                    Image.Load(imageData);
                img.Mutate(x => x.Resize(
                    ThumbWidth, ThumbHeight));
                using var ms =
                    new MemoryStream();
                img.SaveAsPng(ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _log.LogError(
                    ex,
                    "Thumbnail generation"
                    + " failed");
                return null;
            }
        }

        private void ValidateManagerRole()
        {
            var ctx =
                _httpCtx.HttpContext;
            if (ctx == null) return;

            var userIdObj =
                ctx.Items["UserId"];
            if (userIdObj == null) return;

            var userId = (int)userIdObj;
            var hasRole = _users
                .HasRoleAsync(
                    userId,
                    UserRole
                        .InventoryManager)
                .Result;

            if (!hasRole)
            {
                throw
                    new BusinessRuleException(
                        "InsufficientRole",
                        "User does not have"
                        + " the"
                        + " InventoryManager"
                        + " role.");
            }
        }

        private static StockLevel
            ClassifyStockLevel(int qty)
        {
            if (qty <= 0)
                return StockLevel.OutOfStock;
            if (qty <= 5)
                return StockLevel.Critical;
            if (qty <= 10)
                return StockLevel.Low;
            if (qty <= 500)
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
                Description =
                    p.Description,
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
