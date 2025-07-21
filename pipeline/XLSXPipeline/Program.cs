using XLSXPipeline.Extensions;
using XLSXPipeline.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<PipelineService>();
builder.Services.AddPipelineServices();

var host = builder.Build();
host.Run();
