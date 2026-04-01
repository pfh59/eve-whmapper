using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.Metrics;

namespace WHMapper.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddWHMapperTelemetry(this IServiceCollection services, IConfiguration configuration, string applicationName)
    {
        services.AddSingleton<WHMapperStoreMetrics>();

        var otlpEnabled = configuration.GetValue<bool>("Otlp:Enabled", false);
        if (!otlpEnabled)
            return services;

        var enableSystemInstrumentation = configuration.GetValue<bool>("Otlp:EnableSystemInstrumentation", false);

        services.AddOpenTelemetry().WithMetrics(metrics =>
        {
            metrics.ConfigureResource(resource => resource
                .AddService(serviceName: applicationName));

            metrics.AddMeter(configuration.GetValue<string>("WHMapperStoreMeterName")
                ?? throw new InvalidOperationException("WHMapperStoreMeterName is not configured."));

            if (enableSystemInstrumentation)
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation()
                       .AddProcessInstrumentation();
            }

            metrics.AddOtlpExporter((exporterOptions, metricReaderOptions) =>
            {
                exporterOptions.Endpoint = new Uri(configuration["Otlp:Endpoint"]
                    ?? throw new InvalidOperationException("Otlp:Endpoint is not configured."));
                exporterOptions.Protocol = OtlpExportProtocol.Grpc;

                var exportInterval = configuration.GetValue<int>("Otlp:MetricsExportIntervalMilliseconds", 10000);
                metricReaderOptions.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
                {
                    ExportIntervalMilliseconds = exportInterval,
                    ExportTimeoutMilliseconds = configuration.GetValue<int>("Otlp:MetricsExportTimeoutMilliseconds", 10000)
                };
            });
        });

        return services;
    }

    public static async Task InitializeMetricsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var metricsService = scope.ServiceProvider.GetRequiredService<WHMapperStoreMetrics>();

            await metricsService.InitializeTotalsAsync(
                scope.ServiceProvider.GetRequiredService<IWHMapRepository>(),
                scope.ServiceProvider.GetRequiredService<IWHSystemRepository>(),
                scope.ServiceProvider.GetRequiredService<IWHSystemLinkRepository>(),
                scope.ServiceProvider.GetRequiredService<IWHSignatureRepository>(),
                scope.ServiceProvider.GetRequiredService<IWHNoteRepository>(),
                scope.ServiceProvider.GetRequiredService<IWHJumpLogRepository>()
            );

            logger.LogInformation("Metrics initialized successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to initialize metrics, but application will continue.");
        }
    }
}
