using Serilog;

namespace HouseRentMgmt.Api.Infrastructure;

public static class LoggerExtension
{
    public static IHostApplicationBuilder AddCustomLogging(this IHostApplicationBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger: loggerConfiguration, dispose: true);
        return builder;
    }
}
