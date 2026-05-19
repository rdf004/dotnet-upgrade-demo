using System.Web.Http;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Inventory.Services;
using ContosoCommerce.Orders.Services;
using ContosoCommerce.Reporting.Services;
using ContosoCommerce.Users.Services;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.WebApi;

namespace ContosoCommerce.Api.App_Start
{
    /// <summary>
    /// Unity DI container configuration.
    /// Registers all services used across
    /// modules. In .NET 8, Unity is replaced
    /// by the built-in DI container.
    /// </summary>
    public static class UnityConfig
    {
        private static IUnityContainer
            _container;

        public static IUnityContainer Container
        {
            get { return _container; }
        }

        public static void RegisterComponents()
        {
            _container =
                new UnityContainer();

            _container.RegisterType<
                CommerceDbContext>(
                    new HierarchicalLifetimeManager());

            _container.RegisterType<
                IAuditService,
                AuditService>(
                    new HierarchicalLifetimeManager());

            _container.RegisterType<
                IUserService,
                UserService>(
                    new HierarchicalLifetimeManager());

            _container.RegisterType<
                IInventoryService,
                InventoryService>(
                    new HierarchicalLifetimeManager());

            _container.RegisterType<
                IOrderService,
                OrderService>(
                    new HierarchicalLifetimeManager());

            _container.RegisterType<
                IReportService,
                ReportService>(
                    new HierarchicalLifetimeManager());

            _container.RegisterType<
                INotificationService,
                MockNotificationService>(
                    new HierarchicalLifetimeManager());

            _container.RegisterType<
                ICacheService,
                MemoryCacheService>(
                    new ContainerControlledLifetimeManager());

            _container.RegisterType<
                StockMonitorService>(
                    new ContainerControlledLifetimeManager());

            GlobalConfiguration.Configuration
                .DependencyResolver =
                    new UnityDependencyResolver(
                        _container);
        }
    }

    /// <summary>
    /// Mock notification service that logs
    /// instead of sending real emails.
    /// </summary>
    public class MockNotificationService
        : INotificationService
    {
        private static readonly
            log4net.ILog Log =
                log4net.LogManager.GetLogger(
                    typeof(
                        MockNotificationService));

        public System.Threading.Tasks.Task
            SendEmailAsync(
                string toAddress,
                string subject,
                string body)
        {
            Log.InfoFormat(
                "[MOCK EMAIL] To: {0}, "
                + "Subject: {1}",
                toAddress, subject);
            return System.Threading.Tasks
                .Task.CompletedTask;
        }

        public System.Threading.Tasks.Task
            SendStockAlertAsync(
                string productName,
                int currentStock,
                int threshold)
        {
            Log.WarnFormat(
                "[MOCK ALERT] {0}: "
                + "{1} units (threshold: "
                + "{2})",
                productName,
                currentStock,
                threshold);
            return System.Threading.Tasks
                .Task.CompletedTask;
        }

        public System.Threading.Tasks.Task
            SendOrderStatusEmailAsync(
                string toAddress,
                int orderId,
                string newStatus)
        {
            Log.InfoFormat(
                "[MOCK EMAIL] To: {0}, "
                + "Order #{1} -> {2}",
                toAddress,
                orderId,
                newStatus);
            return System.Threading.Tasks
                .Task.CompletedTask;
        }
    }

    /// <summary>
    /// In-memory cache using MemoryCache.
    /// </summary>
    public class MemoryCacheService
        : ICacheService
    {
        private static readonly
            System.Runtime.Caching.MemoryCache
                Cache = System.Runtime.Caching
                    .MemoryCache.Default;

        public T Get<T>(string key)
            where T : class
        {
            return Cache.Get(key) as T;
        }

        public void Set<T>(
            string key,
            T value,
            System.TimeSpan expiration)
            where T : class
        {
            var policy = new System.Runtime
                .Caching.CacheItemPolicy
            {
                AbsoluteExpiration =
                    System.DateTimeOffset
                        .UtcNow.Add(
                            expiration)
            };
            Cache.Set(key, value, policy);
        }

        public void Remove(string key)
        {
            Cache.Remove(key);
        }

        public bool Contains(string key)
        {
            return Cache.Contains(key);
        }
    }
}
