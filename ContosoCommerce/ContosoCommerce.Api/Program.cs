using ContosoCommerce.Api.Middleware;
using ContosoCommerce.Api.ModelBinders;
using ContosoCommerce.Core.Interfaces;
using ContosoCommerce.Data;
using ContosoCommerce.Inventory.Services;
using ContosoCommerce.Orders.Services;
using ContosoCommerce.Reporting.Services;
using ContosoCommerce.Users.Auth;
using ContosoCommerce.Users.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Serilog;
using STJ = System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services.AddControllers(opts =>
    {
        opts.ModelBinderProviders.Insert(
            0,
            new DateTimeModelBinderProvider());
    })
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions
            .PropertyNamingPolicy =
            STJ.JsonNamingPolicy
                .CamelCase;
        opts.JsonSerializerOptions
            .WriteIndented = true;
        opts.JsonSerializerOptions
            .Converters.Add(
            new STJ.Serialization
                .JsonStringEnumConverter());
    });

var connStr = builder.Configuration
    .GetConnectionString("CommerceDb");
builder.Services.AddDbContext<CommerceDbContext>(
    opts => opts.UseSqlServer(connStr));

builder.Services
    .AddHttpContextAccessor();

builder.Services
    .AddAuthentication("TokenAuth")
    .AddScheme<
        AuthenticationSchemeOptions,
        TokenAuthHandler>(
        "TokenAuth", null);
builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();

builder.Services
    .AddScoped<IAuditService, AuditService>();
builder.Services
    .AddScoped<IUserService, UserService>();
builder.Services
    .AddScoped<
        IInventoryService, InventoryService>();
builder.Services
    .AddScoped<IOrderService, OrderService>();
builder.Services
    .AddScoped<IReportService, ReportService>();
builder.Services
    .AddSingleton<
        INotificationService,
        MockNotificationService>();
builder.Services
    .AddSingleton<ICacheService,
        MemoryCacheService>();

builder.Services
    .AddHostedService<StockMonitorWorker>();
builder.Services
    .AddHostedService<ReportSchedulerWorker>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
