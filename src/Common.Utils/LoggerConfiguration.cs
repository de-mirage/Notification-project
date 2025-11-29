using Serilog;

namespace Common.Utils
{
    public static class LoggerConfiguration
    {
        public static Serilog.Core.Logger CreateLogger(string serviceName)
        {
            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {ServiceName} - {Message:lj}{NewLine}{Exception}")
                .WriteTo.File($"logs/{serviceName}.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }
    }
}
