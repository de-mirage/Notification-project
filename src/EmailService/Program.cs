using Microsoft.EntityFrameworkCore;
using NotificationService.Models.DataModels;

var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<global::EmailService.Services.EmailNotificationService>();

// Add the hosted service
builder.Services.AddHostedService<global::EmailService.Workers.EmailWorker>();

var host = builder.Build();
host.Run();
