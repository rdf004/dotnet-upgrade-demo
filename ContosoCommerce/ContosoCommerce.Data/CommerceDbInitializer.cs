using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ContosoCommerce.Core.Enums;
using ContosoCommerce.Data.Entities;

namespace ContosoCommerce.Data
{
    public static class CommerceDbInitializer
    {
        public static void Seed(
            CommerceDbContext context)
        {
            SeedUsers(context);
            SeedCategories(context);
            SeedProducts(context);
            SeedOrders(context);
            context.SaveChanges();
        }

        private static void SeedUsers(
            CommerceDbContext context)
        {
            var hash =
                HashPassword("P@ssw0rd!");

            var users = new List<User>
            {
                new User
                {
                    Email =
                        "admin@contoso.com",
                    PasswordHash = hash,
                    FirstName = "Admin",
                    LastName = "User",
                    Role =
                        UserRole.Administrator,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                },
                new User
                {
                    Email =
                        "inv@contoso.com",
                    PasswordHash = hash,
                    FirstName = "Inventory",
                    LastName = "Manager",
                    Role =
                        UserRole
                            .InventoryManager,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                },
                new User
                {
                    Email =
                        "customer@contoso.com",
                    PasswordHash = hash,
                    FirstName = "Jane",
                    LastName = "Customer",
                    Role =
                        UserRole.Customer,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                },
                new User
                {
                    Email =
                        "reports@contoso.com",
                    PasswordHash = hash,
                    FirstName = "Report",
                    LastName = "Viewer",
                    Role =
                        UserRole.ReportViewer,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                }
            };

            users.ForEach(
                u => context.Users.Add(u));
        }

        private static void SeedCategories(
            CommerceDbContext context)
        {
            var electronics = new Category
            {
                Name = "Electronics",
                Description =
                    "Electronic devices"
                    + " and gadgets"
            };
            var clothing = new Category
            {
                Name = "Clothing",
                Description =
                    "Apparel and accessories"
            };
            var phones = new Category
            {
                Name = "Phones",
                Description =
                    "Mobile phones",
                ParentCategory = electronics
            };

            context.Categories
                .Add(electronics);
            context.Categories
                .Add(clothing);
            context.Categories
                .Add(phones);
        }

        private static void SeedProducts(
            CommerceDbContext context)
        {
            context.SaveChanges();

            var products = new List<Product>
            {
                new Product
                {
                    Name =
                        "Contoso Laptop Pro",
                    Description =
                        "15-inch professional"
                        + " laptop with"
                        + " 16GB RAM",
                    Price = 1299.99m,
                    StockQuantity = 50,
                    Sku = "ELEC-LP-001",
                    CategoryId = 1,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                },
                new Product
                {
                    Name =
                        "Contoso Phone X",
                    Description =
                        "Flagship smartphone"
                        + " with 128GB"
                        + " storage",
                    Price = 899.99m,
                    StockQuantity = 100,
                    Sku = "ELEC-PH-001",
                    CategoryId = 3,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                },
                new Product
                {
                    Name =
                        "Contoso T-Shirt",
                    Description =
                        "Premium cotton"
                        + " branded t-shirt",
                    Price = 29.99m,
                    StockQuantity = 500,
                    Sku = "CLTH-TS-001",
                    CategoryId = 2,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                },
                new Product
                {
                    Name =
                        "Wireless Mouse",
                    Description =
                        "Ergonomic wireless"
                        + " mouse",
                    Price = 49.99m,
                    StockQuantity = 3,
                    Sku = "ELEC-MS-001",
                    CategoryId = 1,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                },
                new Product
                {
                    Name = "USB-C Hub",
                    Description =
                        "7-port USB-C hub"
                        + " with HDMI",
                    Price = 79.99m,
                    StockQuantity = 0,
                    Sku = "ELEC-HB-001",
                    CategoryId = 1,
                    IsActive = true,
                    CreatedAt =
                        DateTime.UtcNow
                }
            };

            products.ForEach(
                p => context.Products.Add(p));
        }

        private static void SeedOrders(
            CommerceDbContext context)
        {
            context.SaveChanges();

            var order = new Order
            {
                UserId = 3,
                Status =
                    OrderStatus.Delivered,
                TotalAmount = 1329.98m,
                ShippingAddress =
                    "123 Main St,"
                    + " Redmond, WA",
                OrderDate =
                    DateTime.UtcNow
                        .AddDays(-7)
            };
            context.Orders.Add(order);
            context.SaveChanges();

            var items = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 1299.99m,
                    LineTotal = 1299.99m
                },
                new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = 3,
                    Quantity = 1,
                    UnitPrice = 29.99m,
                    LineTotal = 29.99m
                }
            };

            items.ForEach(
                i => context.OrderItems
                    .Add(i));

            context.Payments.Add(
                new Payment
                {
                    OrderId = order.Id,
                    Amount = 1329.98m,
                    TransactionId =
                        "TXN-SEED-001",
                    Status = "Completed",
                    PaymentMethod =
                        "CreditCard",
                    ProcessedAt =
                        DateTime.UtcNow
                            .AddDays(-7)
                });
        }

        private static string HashPassword(
            string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(
                Encoding.UTF8.GetBytes(
                    password));
            return Convert.ToHexString(bytes);
        }
    }
}
