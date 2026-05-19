using System;
using System.Diagnostics;
using System.Web;
using log4net;

namespace ContosoCommerce.Api.Modules
{
    /// <summary>
    /// Custom HttpModule that logs the duration
    /// of each request. Registered in web.config.
    /// No equivalent in .NET 8 — must become
    /// middleware.
    /// </summary>
    public class RequestTimingModule
        : IHttpModule
    {
        private static readonly ILog Log =
            LogManager.GetLogger(
                typeof(RequestTimingModule));

        private const string TimerKey =
            "RequestTimer";

        public void Init(
            HttpApplication context)
        {
            context.BeginRequest +=
                OnBeginRequest;
            context.EndRequest +=
                OnEndRequest;
        }

        private void OnBeginRequest(
            object sender, EventArgs e)
        {
            var app =
                (HttpApplication)sender;
            var timer = Stopwatch
                .StartNew();
            app.Context.Items[TimerKey] =
                timer;
        }

        private void OnEndRequest(
            object sender, EventArgs e)
        {
            var app =
                (HttpApplication)sender;
            var timer = app.Context
                .Items[TimerKey]
                    as Stopwatch;

            if (timer == null) return;

            timer.Stop();
            var elapsed =
                timer.ElapsedMilliseconds;
            var path = app.Context.Request
                .Path;
            var method = app.Context.Request
                .HttpMethod;
            var status = app.Context.Response
                .StatusCode;

            Log.InfoFormat(
                "[{0}] {1} {2} - {3}ms",
                status, method,
                path, elapsed);

            if (elapsed > 1000)
            {
                Log.WarnFormat(
                    "SLOW REQUEST: {0} {1} "
                    + "took {2}ms",
                    method, path, elapsed);
            }
        }

        public void Dispose()
        {
        }
    }
}
