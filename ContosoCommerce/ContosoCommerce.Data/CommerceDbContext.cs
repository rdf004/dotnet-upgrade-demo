using Microsoft.EntityFrameworkCore;
using ContosoCommerce.Data.Entities;

namespace ContosoCommerce.Data
{
    public class CommerceDbContext : DbContext
    {
        public CommerceDbContext(
            DbContextOptions<CommerceDbContext>
                options)
            : base(options)
        {
        }

        public DbSet<User> Users
        {
            get; set;
        }

        public DbSet<AuthToken> AuthTokens
        {
            get; set;
        }

        public DbSet<Product> Products
        {
            get; set;
        }

        public DbSet<Category> Categories
        {
            get; set;
        }

        public DbSet<Order> Orders
        {
            get; set;
        }

        public DbSet<OrderItem> OrderItems
        {
            get; set;
        }

        public DbSet<Payment> Payments
        {
            get; set;
        }

        public DbSet<AuditLog> AuditLogs
        {
            get; set;
        }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(
                    c => c.ParentCategoryId)
                .IsRequired(false);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(
                    i => i.ProductId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            modelBuilder.Entity<AuthToken>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            modelBuilder.Entity<AuthToken>()
                .HasIndex(t => t.Token)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            base.OnModelCreating(
                modelBuilder);
        }
    }
}
