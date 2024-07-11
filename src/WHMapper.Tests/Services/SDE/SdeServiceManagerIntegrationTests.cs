using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions;
using WHMapper.Shared.Services.Cache;
using WHMapper.Shared.Services.SDE;
using Xunit.Priority;

namespace WHMapper.Tests.Services.SDE;

[Collection("C4-Services")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class SdeServiceManagerIntegrationTests
{
    private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
    private const string SOLAR_SYSTEM_JIT_NAME = "jit";
    private const int SOLAR_SYSTEM_JITA_ID = 30000142;

    private const string SOLAR_SYSTEM_AMARR_NAME = "Amarr";
    private const string SOLAR_SYSTEM_AMA_NAME = "ama";

    private const string SOLAR_SYSTEM_WH_NAME = "J165153";
    private const string SOLAR_SYSTEM_WH_PARTIAL_NAME = "J1651";

    private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
    private const string SDE_TARGET_DIRECTORY = @"./Resources/SDE/universe";
    private readonly ISDEServiceManager SDEServiceManager;
    private readonly ISDEService SDEService;

    public SdeServiceManagerIntegrationTests()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = configuration.GetConnectionString("RedisConnection");
            option.InstanceName = "WHMapper";
        });

        var provider = services.BuildServiceProvider();

        IDistributedCache? _distriCache = provider.GetRequiredService<IDistributedCache>();

        IFileSystem fileSystem = new FileSystem();
        ICacheService cacheService = new CacheService(new NullLogger<CacheService>(), _distriCache);

        HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(configuration.GetValue<string>("SdeDataSupplier:BaseUrl")) };
        ISDEDataSupplier sDEDataSupplier = new SdeDataSupplier(new NullLogger<SdeDataSupplier>(), httpClient);

        SDEServiceManager = new SDEServiceManager(new NullLogger<SDEServiceManager>(), fileSystem, sDEDataSupplier, cacheService);
        SDEService = new SDEService(new NullLogger<SDEService>(), cacheService);
    }

    [Fact, Priority(0)]
    public async Task WhenFolderIsNonExisting_WhenExtractingNonExistingSdeZip_ReturnsFalse()
    {
        var clearResourcesResult = await SDEServiceManager.ClearSDEResources();
        Assert.True(clearResourcesResult);

        var extractResult = await SDEServiceManager.ExtractSDE();
        Assert.False(extractResult);
    }

    [Fact, Priority(1)]
    public async Task WhenFolderIsNonExisting_WhenDownloadingAndExtractingZip_Succeeds()
    {
        var clearResourcesResult = await SDEServiceManager.ClearSDEResources();
        Assert.True(clearResourcesResult);
        Assert.False(File.Exists(SDE_ZIP_PATH));

        var downloadResult = await SDEServiceManager.DownloadSDE();
        Assert.True(downloadResult);
        Assert.True(File.Exists(SDE_ZIP_PATH));

        var goodExtract = await SDEServiceManager.ExtractSDE();
        Assert.True(goodExtract);
        Assert.True(Directory.Exists(SDE_TARGET_DIRECTORY));
        Assert.True(SDEServiceManager.IsExtractionSuccesful());
    }

    [Fact, Priority(3)]
    public async Task WhenExtractedZipFileIsNonExisting_Importing_Fails()
    {
        var clearRessources = await SDEServiceManager.ClearSDEResources();
        Assert.True(clearRessources);

        var badImport = await SDEServiceManager.BuildCache();
        Assert.False(badImport);
    }

    [Fact, Priority(4)]
    public async Task WhenFolderIsNonExisting_WhenDownloadingExtractingImporting_Succeeds()
    {
        var clearCacheResult = await SDEServiceManager.ClearCache();
        Assert.True(clearCacheResult);

        var clearResourcesResult = await SDEServiceManager.ClearSDEResources();
        Assert.True(clearResourcesResult);
        Assert.False(File.Exists(SDE_ZIP_PATH));

        var downloadResult = await SDEServiceManager.DownloadSDE();
        Assert.True(downloadResult);
        Assert.True(File.Exists(SDE_ZIP_PATH));

        var extractionResult = await SDEServiceManager.ExtractSDE();
        Assert.True(extractionResult);
        Assert.True(Directory.Exists(SDE_TARGET_DIRECTORY));
        Assert.True(SDEServiceManager.IsExtractionSuccesful());

        var importResult = await SDEServiceManager.BuildCache();
        Assert.True(importResult);

        var solarsystems = await SDEService.GetSolarSystemList();
        Assert.NotNull(solarsystems);
        Assert.NotEmpty(solarsystems);
        Assert.Equal(8036, solarsystems.Count()); //k-space+WH

        var solarsystemjumps = await SDEService.GetSolarSystemJumpList();
        Assert.NotNull(solarsystemjumps);
        Assert.NotEmpty(solarsystemjumps);
        Assert.Equal(8036, solarsystemjumps.Count()); //k-space+WH
    }

    [Fact, Priority(50)]
    public async Task Empty_WhenSearchingSystem_ReturnsNull()
    {
        //TEST empty
        var empty_result = await SDEService.SearchSystem("");
        Assert.Null(empty_result);
    }

    [Fact, Priority(50)]
    public async Task System_WhenSearchingSystem_ReturnsValue()
    {
        //TEST JITA
        var jita_result = await SDEService.SearchSystem(SOLAR_SYSTEM_JITA_NAME);
        Assert.NotNull(jita_result);
        Assert.Single(jita_result);
    }

    [Fact, Priority(50)]
    public async Task PartialSystem_WhenSearchingSystem_ReturnsValue()
    {
        //TEST JI for JITA partial
        var ji_result = await SDEService.SearchSystem(SOLAR_SYSTEM_JIT_NAME);
        Assert.NotNull(ji_result);
        Assert.Contains(ji_result, x => x.Name.Contains(SOLAR_SYSTEM_JITA_NAME, StringComparison.OrdinalIgnoreCase));
    }

    [Fact, Priority(50)]
    public async Task System2_WhenSearchingSystem_ReturnsValue()
    {
        //TEST AMARR
        var amarr_result = await SDEService.SearchSystem(SOLAR_SYSTEM_AMARR_NAME);
        Assert.NotNull(amarr_result);
        Assert.Single(amarr_result);
    }

    [Fact, Priority(50)]
    public async Task PartialSystem2_WhenSearchingSystem_ReturnsValue()
    {
        //TEST AMA for AMARR partial
        var ama_result = await SDEService.SearchSystem(SOLAR_SYSTEM_AMA_NAME);
        Assert.NotNull(ama_result);
        Assert.Contains(ama_result, x => x.Name.Contains(SOLAR_SYSTEM_AMARR_NAME, StringComparison.OrdinalIgnoreCase));
    }

    [Fact, Priority(50)]
    public async Task WH_WhenSearchingSystem_ReturnsValue()
    {
        //TEST WH
        var wh_result = await SDEService.SearchSystem(SOLAR_SYSTEM_WH_NAME);
        Assert.NotNull(wh_result);
        Assert.Single(wh_result);

        var wh_partial_result = await SDEService.SearchSystem(SOLAR_SYSTEM_WH_PARTIAL_NAME);
        Assert.NotNull(wh_partial_result);
        Assert.Contains(wh_partial_result, x => x.Name.Contains(SOLAR_SYSTEM_WH_NAME, StringComparison.OrdinalIgnoreCase));
    }

    [Fact, Priority(50)]
    public async Task InvalidValue_WhenSearching_ReturnsNull()
    {
        Assert.NotNull(SDEServiceManager);
        var bad_result = await SDEService.SearchSystemById(-1);
        Assert.Null(bad_result);
    }

    [Fact, Priority(50)]
    public async Task InvalidValue2_WhenSearching_ReturnsNull()
    {
        var empty_result = await SDEService.SearchSystemById(0);
        Assert.Null(empty_result);
    }

    [Fact, Priority(50)]
    public async Task Jita_WhenSearching_ReturnsNull()
    {
        //TEST JITA
        var jita_result = await SDEService.SearchSystemById(SOLAR_SYSTEM_JITA_ID);
        Assert.NotNull(jita_result);
        Assert.Equal(SOLAR_SYSTEM_JITA_ID, jita_result.SolarSystemID);
    }

    [Fact, Priority(50)]
    public async Task WH_WhenSearching_ReturnsNull()
    {
        //TEST WH
        var wh_result = await SDEService.SearchSystemById(31001123);
        Assert.NotNull(wh_result);
        Assert.Equal(31001123, wh_result.SolarSystemID);
    }
}


