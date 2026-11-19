using System.Net;
using Microsoft.Extensions.Http.Resilience;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveScoutAPI;
using WHMapper.Services.Anoik;
using WHMapper.Services.SDE;
using WHMapper.Services.WHIntel;
using WHMapper.Services.ZKillboardAPI;

namespace WHMapper.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddExternalApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IEveAPIServices, EveAPIServices>(client =>
        {
            client.BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddStandardResilienceHandler(options => ConfigureEsiResilience(options));

        services.AddHttpClient<ICharacterServices, CharacterServices>(client =>
        {
            client.BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddStandardResilienceHandler(options => ConfigureEsiResilience(options));

        services.AddHttpClient<IEveScoutAPIServices, EveScoutAPIServices>(client =>
        {
            client.BaseAddress = new Uri(EveScoutAPIServiceConstants.EveScoutUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 2;
            options.Retry.UseJitter = true;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IZKillboardAPIServices, ZKillboardAPIServices>(client =>
        {
            client.BaseAddress = new Uri(ZKillboardAPIServiceConstants.ZKillboardUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(ZKillboardAPIServiceConstants.USER_AGENT);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        // zKillboard requires compressed responses; automatic decompression sends Accept-Encoding.
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        .AddStandardResilienceHandler(options =>
        {
            // One retry only: zKillboard asks clients to avoid bursts; Retry-After is honoured by the handler.
            options.Retry.MaxRetryAttempts = 1;
            options.Retry.UseJitter = true;
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(20);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(45);
        });

        // Optional AI model of the intel report: any OpenAI-compatible endpoint, cloud or local container.
        var intelAiOptions = configuration.GetSection(WHIntelAiOptions.SECTION_NAME).Get<WHIntelAiOptions>() ?? new WHIntelAiOptions();
        services.AddSingleton(sp => WHIntelAiReporterFactory.Create(intelAiOptions, sp.GetRequiredService<ILoggerFactory>()));

        services.AddScoped<IAnoikDataSupplier>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var jsonFilePath = config["AnoikDataSupplier:JsonFilePath"]
                ?? throw new InvalidOperationException("JsonFilePath is not configured in AnoikDataSupplier settings.");
            return new AnoikJsonDataSupplier(jsonFilePath);
        });

        services.AddScoped<IAnoikServices, AnoikServices>();
        services.AddScoped<ISDEService, SDEService>();
        services.AddScoped<ISDEServiceManager, SDEServiceManager>();
        services.AddScoped<ISDEDataSupplier, SdeDataSupplier>();
        services.AddSingleton<ISDEInitializationState, SDEInitializationState>();
        services.AddHttpClient<ISDEDataSupplier, SdeDataSupplier>(client =>
        {
            var baseUrl = configuration["SdeDataSupplier:BaseUrl"]
                ?? throw new InvalidOperationException("BaseUrl is not configured in SdeDataSupplier settings.");
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }

    private static void ConfigureEsiResilience(HttpStandardResilienceOptions options)
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.UseJitter = true;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    }
}
