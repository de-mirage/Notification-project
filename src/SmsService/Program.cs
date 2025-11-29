using Microsoft.Extensions.Hosting;
using SmsService.Workers;

var builder = Host.CreateApplicationBuilder(args);

// Add custom services
builder.Services.AddScoped<SmsService.Services.SmsService>();

// Add the hosted service
builder.Services.AddHostedService<SmsWorker>();

var host = builder.Build();
host.Run();
