using WHMapper.Models.DTO;
using WHMapper.Repositories.WHInstances;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMapAccesses;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.BrowserClientIdProvider;
using WHMapper.Services.BrowserClientIdProvider.Extension;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHColor;
using WHMapper.Services.WHSignature;
using WHMapper.Services.WHSignatures;

namespace WHMapper.Extensions;

public static class MapperServiceExtensions
{
    public static IServiceCollection AddMapperServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IWHMapRepository, WHMapRepository>();
        services.AddScoped<IWHSystemRepository, WHSystemRepository>();
        services.AddScoped<IWHSignatureRepository, WHSignatureRepository>();
        services.AddScoped<IWHSystemLinkRepository, WHSystemLinkRepository>();
        services.AddScoped<IWHNoteRepository, WHNoteRepository>();
        services.AddScoped<IWHRouteRepository, WHRouteRepository>();
        services.AddScoped<IWHJumpLogRepository, WHJumpLogRepository>();
        services.AddScoped<IWHInstanceRepository, WHInstanceRepository>();
        services.AddScoped<IWHMapAccessRepository, WHMapAccessRepository>();

        // Mapper services
        services.AddSingleton<IBrowserClientIdProvider, BrowserClientIdProvider>();
        services.AddScoped<ClientUID>();
        services.AddSingleton<IEveMapperUserManagementService, EveMapperUserManagementService>();
        services.AddScoped<IEveMapperService, EveMapperService>();
        services.AddScoped<IEveMapperCacheService, EveMapperCacheService>();
        services.AddScoped<IEveMapperAccessHelper, EveMapperAccessHelper>();
        services.AddScoped<IEveMapperInstanceService, EveMapperInstanceService>();
        services.AddScoped<IEveMapperTracker, EveMapperTracker>();
        services.AddScoped<IEveMapperSearch, EveMapperSearch>();
        services.AddScoped<IEveMapperHelper, EveMapperHelper>();
        services.AddScoped<IEveMapperRoutePlannerHelper, EveMapperRoutePlannerHelper>();
        services.AddScoped<IWHSignatureHelper, WHSignatureHelper>();
        services.AddScoped<IWHColorHelper, WHColorHelper>();
        services.AddScoped<IEveMapperRealTimeService, EveMapperRealTimeService>();
        services.AddScoped<IPasteServices, PasteServices>();

        return services;
    }
}
