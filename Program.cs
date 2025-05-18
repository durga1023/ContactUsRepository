using ContactApplication.Repositories;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ContactFormRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ContactFormDb")));
// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ContactFormPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Partition key: use IP address as string, fallback to "unknown"
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1, // Allow 5 requests
                Window = TimeSpan.FromMinutes(1), // Per 1 minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});
var app = builder.Build();

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
app.Run();
