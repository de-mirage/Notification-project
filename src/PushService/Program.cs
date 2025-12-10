using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models.DataModels;
using PushService.Workers;

var builder = Host.CreateApplicationBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom services
builder.Services.AddScoped<PushService.Services.PushService>();

// Add the hosted service
builder.Services.AddHostedService<PushWorker>();

var host = builder.Build();
host.Run();
