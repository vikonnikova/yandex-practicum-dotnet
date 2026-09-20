using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace Bookings.Api.Telemetry;

internal static class ObservabilityExtensions
{
    public const string MetricsPath = "/metrics";

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration["ServiceName"] ?? "bookings-service";
        var otlpEndpoint = builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317";

        builder.Host.UseSerilog((ctx, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration)
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(new CompactJsonFormatter()));

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName: serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = httpContext => httpContext.Request.Path != MetricsPath;
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        return builder;
    }

    public static WebApplication MapObservabilityEndpoints(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint(MetricsPath);
        return app;
    }
}