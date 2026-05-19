using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ContosoCommerce.Api.Middleware;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<
        RequestTimingMiddleware> _log;

    public RequestTimingMiddleware(
        RequestDelegate next,
        ILogger<RequestTimingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;

        _log.LogInformation(
            "Request started: {Method} {Path}",
            method, path);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var status = context
                .Response.StatusCode;
            _log.LogInformation(
                "Request finished: "
                + "{Method} {Path} "
                + "=> {Status} in {Ms}ms",
                method, path,
                status,
                sw.ElapsedMilliseconds);
        }
    }
}
