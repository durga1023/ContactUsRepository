using ContactApplication.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using log4net;
using log4net.Config;
using System.Reflection;
using System.Text.Json;
using ContactApplication.Repositories.Interfaces;

var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
ILog log = LogManager.GetLogger(typeof(Program));

XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
var builder = WebApplication.CreateBuilder(args);

var connectionString = await AwsSecretsHelper.GetSecretValueAsync("contactformcredentials", "us-east-2", "CONNECTION_STRING");
log.Info("Successfully fetched connection string.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IContactFormRepository, ContactFormRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
// Add rate limiting
log.Info("Configuring rate limiting policy.");
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ContactFormPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Partition key: use IP address as string, fallback to "unknown"
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3, // Allow 3 requests
                Window = TimeSpan.FromMinutes(1), // Per 1 minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});
var app = builder.Build();

// Global exception handler
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        log.Error("Unhandled exception occurred during request processing.", ex);
        throw;
    }
});
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Contact}/{action=Contact}/{id?}");

app.UseRateLimiter();

log.Info("Application has started.");

app.Run();
