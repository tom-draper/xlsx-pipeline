using NLog;
using NLog.Targets;
using NLog.Extensions.Logging;
using XLSXPipeline.Extensions;
using XLSXPipeline.Services;

var logger = LogManager.Setup().LoadConfiguration(builder =>
{
    var consoleTarget = new ConsoleTarget("console")
    {
        Layout = "${longdate} ${level:uppercase=true}: ${message} ${exception:format=toString}"
    };
    builder.Configuration.AddTarget(consoleTarget);

    var fileTarget = new FileTarget("file")
    {
        FileName = "logs/application-${shortdate}.log",
        Layout = "${longdate} ${level:uppercase=true}: ${message}${when:when=length('${exception:format=toString}')>0:inner=${newline}${exception:format=toString}}"
    };
    builder.Configuration.AddTarget(fileTarget);

    builder.Configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget, "*");
    builder.Configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget, "*");

}).GetCurrentClassLogger();

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