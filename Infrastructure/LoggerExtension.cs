namespace HouseRentMgmt.Api.Infrastructure;

public static class LoggerExtension
{
    public static IHostApplicationBuilder AddCustomLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        return builder;
    }
}
