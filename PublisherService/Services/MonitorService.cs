using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Core;
using System.Diagnostics;
using System.Reflection;
using Serilog.Sinks.Grafana.Loki;

namespace Monitoring;
public static class MonitorService
{
    public static readonly string ServiceName = Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
    public static TracerProvider TracerProvider;
    public static ActivitySource ActivitySource = new ActivitySource(ServiceName);


    public static Serilog.ILogger Log => Serilog.Log.Logger;

    static MonitorService()
    {

        TracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddConsoleExporter()
            .AddZipkinExporter(config =>
            {
                config.Endpoint = new Uri(Environment.GetEnvironmentVariable("ZIPKIN_URL") ?? "http://localhost:9411/api/v2/spans");
            })
            .AddSource(ActivitySource.Name)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
            .SetSampler(new AlwaysOnSampler())
            .Build();

        
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", ServiceName)
            .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
            .WriteTo.GrafanaLoki(Environment.GetEnvironmentVariable("LOKI_URL") ?? "http://localhost:3100",
                labels: new[] { new LokiLabel { Key = "service_name", Value = ServiceName } }
            )
            .CreateLogger();
    }
}
