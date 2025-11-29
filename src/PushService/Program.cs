using Microsoft.Extensions.Hosting;
using PushService.Workers;

var builder = Host.CreateApplicationBuilder(args);

// Add custom services
builder.Services.AddScoped<PushService.Services.PushService>();

// Add the hosted service
builder.Services.AddHostedService<PushWorker>();

var host = builder.Build();
host.Run();
