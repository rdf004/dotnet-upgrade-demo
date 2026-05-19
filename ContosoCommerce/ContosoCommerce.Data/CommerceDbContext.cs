using System.Data.Entity;
using ContosoCommerce.Data.Entities;

namespace ContosoCommerce.Data
{
    /// <summary>
    /// Shared EF6 context used by all modules.
    /// Contains DbSets for every entity in the
    /// system. This tight coupling is a key
    /// migration challenge.
    /// </summary>
    public class CommerceDbContext : DbContext
    {
        public CommerceDbContext()
            : base("name=CommerceDb")
        {
            Configuration.LazyLoadingEnabled =
                true;
            Configuration
                .ProxyCreationEnabled = true;
        }

        public DbSet<User> Users { get; set; }

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

        public DbSet<Order> Orders { get; set; }

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
            DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(
                    c => c.ParentCategoryId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithRequired(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderItem>()
                .HasRequired(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AuthToken>()
                .HasRequired(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Payment>()
                .HasRequired(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}
