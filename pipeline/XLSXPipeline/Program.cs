using NLog;
using NLog.Extensions.Logging;
using XLSXPipeline.Extensions;
using XLSXPipeline.Services;

var logger = LogManager.GetCurrentClassLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            logging.AddNLog();
        })
        .ConfigureServices(services =>
        {
            services.AddHostedService<PipelineService>();
            services.AddPipelineServices();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
