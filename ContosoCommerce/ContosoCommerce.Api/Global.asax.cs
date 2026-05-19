using System;
using System.Data.Entity;
using System.Web;
using System.Web.Http;
using ContosoCommerce.Api.App_Start;
using ContosoCommerce.Data;
using ContosoCommerce.Inventory.Services;
using ContosoCommerce.Reporting.Services;
using log4net;

namespace ContosoCommerce.Api
{
    /// <summary>
    /// Application entry point. Configures
    /// routing, DI, database initialization,
    /// log4net, and background services.
    /// </summary>
    public class WebApiApplication
        : HttpApplication
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(WebApiApplication));

        private static StockMonitorService
            _stockMonitor;
        private static ReportScheduler
            _reportScheduler;

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator
                .Configure();

            Log.Info(
                "ContosoCommerce starting.");

            Database.SetInitializer(
                new CommerceDbInitializer());

            GlobalConfiguration.Configure(
                WebApiConfig.Register);

            UnityConfig.RegisterComponents();

            StartBackgroundServices();

            Log.Info(
                "ContosoCommerce started.");
        }

        protected void Application_End()
        {
            Log.Info(
                "ContosoCommerce shutting "
                + "down.");

            StopBackgroundServices();
        }

        private static void
            StartBackgroundServices()
        {
            try
            {
                var container =
                    UnityConfig.Container;

                _stockMonitor =
                    container.Resolve(
                        typeof(
                            StockMonitorService))
                    as StockMonitorService;
                _stockMonitor?.Start();

                _reportScheduler =
                    new ReportScheduler();
                _reportScheduler.Start();

                Log.Info(
                    "Background services "
                    + "started.");
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Failed to start "
                    + "background services",
                    ex);
            }
        }

        private static void
            StopBackgroundServices()
        {
            _stockMonitor?.Stop();
            _stockMonitor?.Dispose();
            _reportScheduler?.Stop();
            _reportScheduler?.Dispose();
        }
    }
}
