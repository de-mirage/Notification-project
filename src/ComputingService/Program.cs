using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Orleans
builder.Services.AddOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering()
               .Configure<ClusterOptions>(options =>
               {
                   options.ClusterId = builder.Configuration.GetValue<string>("Orleans:ClusterId", "dev");
                   options.ServiceId = builder.Configuration.GetValue<string>("Orleans:ServiceId", "ComputingService");
               })
               .Configure<EndpointOptions>(options =>
               {
                   options.AdvertisedIPAddress = System.Net.IPAddress.Loopback;
                   options.SiloPort = 1111;
                   options.GatewayPort = 30000;
               });
});

// Add Orleans computing service
builder.Services.AddScoped<ComputingService.Services.OrleansComputingService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
