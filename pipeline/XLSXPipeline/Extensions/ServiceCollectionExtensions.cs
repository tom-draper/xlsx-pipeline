using XLSXPipeline.Services;

namespace XLSXPipeline.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPipelineServices(this IServiceCollection services)
        {
            services.AddSingleton<IPipelineLoader, PipelineLoader>();
            services.AddSingleton<IScheduledPipelineFactory, ScheduledPipelineFactory>();
            services.AddSingleton<ITriggerParser, TriggerParser>();
            services.AddSingleton<ISchedulerService, SchedulerService>();
            services.AddSingleton<IFileWatcherService, FileWatcherService>();
            services.AddSingleton<IPipelineExecutor, PipelineExecutor>();
            services.AddSingleton<IPipelineService, PipelineService>();

            return services;
        }
    }
}