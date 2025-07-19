using XLSXPipeline;
using XLSXPipeline.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddPipelineServices();

var host = builder.Build();
host.Run();
