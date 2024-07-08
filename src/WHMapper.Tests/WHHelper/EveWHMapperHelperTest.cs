using System.IO.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.Anoik;
using WHMapper.Services.Cache;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveMapper;
using WHMapper.Services.EveOnlineUserInfosProvider;
using WHMapper.Services.SDE;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;

[Collection("C6-WHHelper")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class EveWHMapperHelperTest
{
    private const int DEFAULT_MAP_ID = 1;
    private const int SOLAR_SYSTEM_JITA_ID = 30000142;
    private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
    private const char SOLAR_SYSTEM_EXTENSION_NAME = 'B';
    private const string CONSTELLATION_JITA_NAME = "Kimotoro";
    private const int CONSTALLATION_JITA_ID = 20000020;
    private const string REGION_JITA_NAME = "The Forge";

    private const string SOLAR_SYSTEM_AMAMAKE_NAME = "Amamake";
    private const string CONSTELLATION_AMAMAKE_NAME = "Hed";
    private const string REGION_AMAMAKE_NAME = "Heimatar";

    private const string SOLAR_SYSTEM_FDMLJ_NAME = "FD-MLJ";
    private const string CONSTELLATION_FDMLJ_NAME = "Z-6NQ6";
    private const string REGION_FDMLJ_NAME = "Syndicate";

    private const int SOLAR_SYSTEM_WH_ID = 31001123;
    private const string SOLAR_SYSTEM_WH_NAME = "J165153";
    private const int CONSTELLATION_WH_ID = 21000113;
    private const string CONSTELLATION_WH_NAME = "C-C00113";
    private const string REGION_WH_NAME = "C-R00012";
    private const EveSystemType SOLAR_SYSTEM_WH_CLASS = EveSystemType.C3;
    private const WHEffect SOLAR_SYSTEM_WH_EFFECT = WHEffect.Pulsar;
    private const string SOLAR_SYSTEM_WH_STATICS = "D845";

    private const string REGION_WH_C1_NAME = "A-R00001";
    private const string SOLAR_SYSTEM_WH_C1_NAME = "J100744";

    private const string REGION_WH_C2_NAME = "B-R00004";
    private const string SOLAR_SYSTEM_WH_C2_NAME = "J101524";

    private const string REGION_WH_C4_NAME = "D-R00016";
    private const string SOLAR_SYSTEM_WH_C4_NAME = "J104754";

    private const string REGION_WH_C5_NAME = "E-R00024";
    private const string SOLAR_SYSTEM_WH_C5_NAME = "J103251";

    private const string REGION_WH_C6_NAME = "F-R00030";
    private const string SOLAR_SYSTEM_WH_C6_NAME = "J104859";

    private const string REGION_WH_THERA_NAME = "G-R00031";
    private const string SOLAR_SYSTEM_WH_THERA_NAME = "Thera";

    private const string REGION_WH_C13_NAME = "H-R00032";
    private const string SOLAR_SYSTEM_WH_C13_NAME = "J000487";

    private const string REGION_WH_POCHVEN_NAME = "Pochven";
    private const string SOLAR_SYSTEM_WH_POCHVEN_NAME = "Archee";

    private const string REGION_SPECIAL = "K-R00033";
    private const string C14_NAME = "J055520";
    private const string C15_NAME = "J110145";
    private const string C16_NAME = "J164710";
    private const string C17_NAME = "J200727";
    private const string C18_NAME = "J174618";

    //https://github.com/pfh59/eve-whmapper/issues/207
    private const string C4_NAME_BUG_207 = "J1226-0";

    private IEveMapperHelper _whEveMapper = null!;

    public EveWHMapperHelperTest()
    {
        //Create DB Context
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddHttpClient();

        services.AddDbContextFactory<WHMapperContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection"))
            );

        services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = configuration.GetConnectionString("RedisConnection");
                option.InstanceName = "WHMapper";
            });

        var provider = services.BuildServiceProvider();
        var httpclientfactory = provider.GetService<IHttpClientFactory>();

        IDbContextFactory<WHMapperContext>? _contextFactory = provider.GetService<IDbContextFactory<WHMapperContext>>();
        IDistributedCache? _distriCache = provider.GetService<IDistributedCache>();

        ILogger<SDEService> loggerSDE = new NullLogger<SDEService>();
        ILogger<AnoikServices> loggerAnoik = new NullLogger<AnoikServices>();
        ILogger<EveMapperHelper> loggerMapperHelper = new NullLogger<EveMapperHelper>();
        ILogger<EveAPIServices> loggerAPI = new NullLogger<EveAPIServices>();
        ILogger<WHNoteRepository> loggerWHNoteRepository = new NullLogger<WHNoteRepository>();
        ILogger<CacheService> loggerCacheService = new NullLogger<CacheService>();
        ILogger<EveMapperService> loggerEveMapperEntity = new NullLogger<EveMapperService>();
        ILogger<SdeDataSupplier> loggerSdeDataSupplier = new NullLogger<SdeDataSupplier>();
        ILogger<EveMapperCacheService> loggereveMapperCacheService = new NullLogger<EveMapperCacheService>();

        if (httpclientfactory != null && _contextFactory != null && _distriCache != null)
        {
            ICacheService cacheService = new CacheService(loggerCacheService, _distriCache);
            IEveMapperCacheService eveMapperCacheService = new EveMapperCacheService(loggereveMapperCacheService, cacheService);

            var userInfoService = new EveUserInfosServices(null!);
            var apiServices = new EveAPIServices(loggerAPI, httpclientfactory, new TokenProvider(), userInfoService);
            var mapperEntity = new EveMapperService(loggerEveMapperEntity, eveMapperCacheService, apiServices);

            IFileSystem fileSystem = new FileSystem();
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(configuration.GetValue<string>("SdeDataSupplier:BaseUrl")) };
            ISDEDataSupplier dataSupplier = new SdeDataSupplier(loggerSdeDataSupplier, httpClient);

            ISDEService sDEServices = new SDEService(loggerSDE, cacheService);
            IAnoikServices anoikServices = new AnoikServices(loggerAnoik, new AnoikJsonDataSupplier(@"./Resources/Anoik/static.json"));
            IWHNoteRepository wHNoteRepository = new WHNoteRepository(loggerWHNoteRepository, _contextFactory);

            _whEveMapper = new EveMapperHelper(loggerMapperHelper, mapperEntity, sDEServices, anoikServices, wHNoteRepository);
        }
    }

    [Fact, Priority(1)]
    public Task Is_Wormhole()
    {
        bool not_wh_result = _whEveMapper.IsWormhole(SOLAR_SYSTEM_JITA_NAME);
        Assert.False(not_wh_result);

        bool is_wh_result = _whEveMapper.IsWormhole(SOLAR_SYSTEM_WH_NAME);
        Assert.True(is_wh_result);

        bool bad_result = _whEveMapper.IsWormhole("BAD_NAME");
        Assert.False(bad_result);

        bool bad_empty_result = _whEveMapper.IsWormhole(string.Empty);
        Assert.False(bad_empty_result);

        return Task.CompletedTask;
    }

    [Fact, Priority(2)]
    public async Task Get_Wormhole_Class()
    {
        var result_C3_Bis = await _whEveMapper.GetWHClass(new SystemEntity(31001123, SOLAR_SYSTEM_WH_NAME, CONSTELLATION_WH_ID, -1.0f, new int[] { }));
        Assert.Equal(EveSystemType.C3, result_C3_Bis);

        var result_HS = await _whEveMapper.GetWHClass(REGION_JITA_NAME, "UNUSED", SOLAR_SYSTEM_JITA_NAME, 1.0f);
        Assert.Equal(EveSystemType.HS, result_HS);

        var result_LS = await _whEveMapper.GetWHClass(REGION_AMAMAKE_NAME, "UNUSED", SOLAR_SYSTEM_AMAMAKE_NAME, 0.4f);
        Assert.Equal(EveSystemType.LS, result_LS);

        var result_NS = await _whEveMapper.GetWHClass(REGION_FDMLJ_NAME, "UNUSED", SOLAR_SYSTEM_FDMLJ_NAME, 0.0f);
        Assert.Equal(EveSystemType.NS, result_NS);

        var result_C1 = await _whEveMapper.GetWHClass(REGION_WH_C1_NAME, "UNUSED", SOLAR_SYSTEM_WH_C1_NAME, -1.0f);
        Assert.Equal(EveSystemType.C1, result_C1);

        var result_C2 = await _whEveMapper.GetWHClass(REGION_WH_C2_NAME, "UNUSED", SOLAR_SYSTEM_WH_C2_NAME, -1.0f);
        Assert.Equal(EveSystemType.C2, result_C2);

        var result_C3 = await _whEveMapper.GetWHClass(REGION_WH_NAME, "UNUSED", SOLAR_SYSTEM_WH_NAME, -1.0f);
        Assert.Equal(EveSystemType.C3, result_C3);

        var result_C4 = await _whEveMapper.GetWHClass(REGION_WH_C4_NAME, "UNUSED", SOLAR_SYSTEM_WH_C4_NAME, -1.0f);
        Assert.Equal(EveSystemType.C4, result_C4);

        var result_C5 = await _whEveMapper.GetWHClass(REGION_WH_C5_NAME, "UNUSED", SOLAR_SYSTEM_WH_C5_NAME, -1.0f);
        Assert.Equal(EveSystemType.C5, result_C5);

        var result_C6 = await _whEveMapper.GetWHClass(REGION_WH_C6_NAME, "UNUSED", SOLAR_SYSTEM_WH_C6_NAME, -1.0f);
        Assert.Equal(EveSystemType.C6, result_C6);

        var result_THERA = await _whEveMapper.GetWHClass(REGION_WH_THERA_NAME, "UNUSED", SOLAR_SYSTEM_WH_THERA_NAME, -1.0f);
        Assert.Equal(EveSystemType.Thera, result_THERA);

        var result_C13 = await _whEveMapper.GetWHClass(REGION_WH_C13_NAME, "UNUSED", SOLAR_SYSTEM_WH_C13_NAME, -1.0f);
        Assert.Equal(EveSystemType.C13, result_C13);

        var result_C14 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C14_NAME, -1.0f);
        Assert.Equal(EveSystemType.C14, result_C14);

        var result_C15 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C15_NAME, -1.0f);
        Assert.Equal(EveSystemType.C15, result_C15);

        var result_C16 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C16_NAME, -1.0f);
        Assert.Equal(EveSystemType.C16, result_C16);

        var result_C17 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C17_NAME, -1.0f);
        Assert.Equal(EveSystemType.C17, result_C17);

        var result_C18 = await _whEveMapper.GetWHClass(REGION_SPECIAL, "UNUSED", C18_NAME, -1.0f);
        Assert.Equal(EveSystemType.C18, result_C18);

        var result_POCHVEN = await _whEveMapper.GetWHClass(REGION_WH_POCHVEN_NAME, "UNUSED", SOLAR_SYSTEM_WH_POCHVEN_NAME, -1.0f);
        Assert.Equal(EveSystemType.Pochven, result_POCHVEN);
    }

    [Fact, Priority(3)]
    public async Task Define_Eve_System_Node_Model()
    {
        var jita_result = await _whEveMapper.DefineEveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, 1.0f));
        Assert.NotNull(jita_result);
        Assert.Equal(EveSystemType.HS, jita_result.SystemType);

        var wh_result = await _whEveMapper.DefineEveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_WH_NAME, -1.0f));
        Assert.Equal(SOLAR_SYSTEM_WH_CLASS, wh_result.SystemType);
        Assert.NotEqual(WHEffect.None, wh_result.Effect);
        Assert.Equal(SOLAR_SYSTEM_WH_EFFECT, wh_result.Effect);
    }

    [Fact, Priority(4)]
    public async Task Test_BUG_207()
    {
        var result = await _whEveMapper.DefineEveSystemNodeModel(new WHSystem(DEFAULT_MAP_ID, 31001531, C4_NAME_BUG_207, -1.0f));
        Assert.Equal(EveSystemType.C4, result.SystemType);
    }

    [Fact, Priority(5)]
    public async Task Is_Route_Via_WH()
    {
        var src = new SystemEntity(SOLAR_SYSTEM_WH_ID, SOLAR_SYSTEM_WH_NAME, CONSTELLATION_WH_ID, -1.0f, new int[] { });//fake for test
        var dst = new SystemEntity(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, CONSTALLATION_JITA_ID, -1.0f, new int[] { 50001248 });//fake for test

        var result = await _whEveMapper.IsRouteViaWH(src, dst);
        Assert.True(result);

        var src2 = new SystemEntity(SOLAR_SYSTEM_JITA_ID, SOLAR_SYSTEM_JITA_NAME, CONSTALLATION_JITA_ID, -1.0f, new int[] { 50001248 });
        var dst2 = new SystemEntity(30000140, "Maurasi", CONSTALLATION_JITA_ID, -1.0f, new int[] { 50000802 });

        var result_wh = await _whEveMapper.IsRouteViaWH(src2, dst2);
        Assert.False(result_wh);
    }
}
