using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Pages.Mapper.Administration;
using WHMapper.Services.Anoik;
using WHMapper.Services.Cache;
using WHMapper.Services.SDE;
using Xunit.Priority;

namespace WHMapper.Tests.Services;


[Collection("C4-Services")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class SDEUniverseTest
{

    private const string SOLAR_SYSTEM_JITA_NAME = "Jita";
    private const string SOLAR_SYSTEM_JIT_NAME = "jit";
    private const int SOLAR_SYSTEM_JITA_ID = 30000142;

    private const string SOLAR_SYSTEM_AMARR_NAME = "Amarr";
    private const string SOLAR_SYSTEM_AMA_NAME = "ama";

    private const string SOLAR_SYSTEM_WH_NAME = "J165153";
    private const string SOLAR_SYSTEM_WH_PARTIAL_NAME = "J1651";

    private const string SDE_ZIP_PATH = @"./Resources/SDE/sde.zip";
    //private const string SDE_ZIP_MOVE_PATH = @"./Resources/SDE/sde2.zip";
    private const string SDE_TARGET_DIRECTORY= @"./Resources/SDE/universe";

    private const string SDE_CHECKSUM_FILE = @"./Resources/SDE/checksum";
    private const string SDE_CHECKSUM_CURRENT_FILE = @"./Resources/SDE/currentchecksum";

    private readonly ISDEServices? _services = null!;

    public SDEUniverseTest()
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
        
        IDistributedCache? _distriCache = provider.GetService<IDistributedCache>();
        
        if(_distriCache != null)
        {
        ILogger<SDEServices> logger = new NullLogger<SDEServices>();
        ILogger<CacheService> loggerCache = new NullLogger<CacheService>();
        _services = new SDEServices(logger,new CacheService(loggerCache,_distriCache));
        }

    }


    [Fact, Priority(1)]
    public async Task Is_New_SDE_Available()
    {
        Assert.NotNull(_services);

        if (Directory.Exists(SDE_TARGET_DIRECTORY))
            Directory.Delete(SDE_TARGET_DIRECTORY,true);

        if(File.Exists(SDE_CHECKSUM_CURRENT_FILE))
            File.Delete(SDE_CHECKSUM_CURRENT_FILE);

        if(File.Exists(SDE_CHECKSUM_FILE))
            File.Delete(SDE_CHECKSUM_FILE);

        //from scratch
        Assert.False(_services.ExtractSuccess);

        bool newSDEavailable = await _services.IsNewSDEAvailable();
        Assert.True(newSDEavailable);

        bool noSDEavailable = await _services.IsNewSDEAvailable();
        Assert.False(noSDEavailable);

        //currentcheck not equal to download checksum
        File.WriteAllText(SDE_CHECKSUM_CURRENT_FILE,"azerty");
        bool otherSDEavailable = await _services.IsNewSDEAvailable();
        Assert.True(otherSDEavailable);
    }

    [Fact, Priority(2)]
    public async Task Download_And_Extrat_SDE()
    {
        Assert.NotNull(_services);

        bool clearRessources = await _services.ClearSDERessources();
        Assert.True(clearRessources);


        bool badExtract = await _services.ExtractSDE();
        Assert.False(badExtract);

        bool downloadSuccess = await _services.DownloadSDE();
        Assert.True(downloadSuccess);
        Assert.True(File.Exists(SDE_ZIP_PATH));

        bool goodExtract = await _services.ExtractSDE();
        Assert.True(goodExtract);
        Assert.True(Directory.Exists(SDE_TARGET_DIRECTORY));
        Assert.True(_services.ExtractSuccess);
    }



    [Fact, Priority(3)]
    public async Task Import_SDE_And_Get()
    {
        Assert.NotNull(_services);

        bool clearRessources = await _services.ClearSDERessources();
        Assert.True(clearRessources);

        bool badImport = await _services.Import();
        Assert.False(badImport);

        await Download_And_Extrat_SDE();
        
        bool cacheClear = await _services.ClearCache();
        Assert.True(cacheClear);

        var solarsystems = await _services.GetSolarSystemList();
        Assert.NotNull(solarsystems);
        Assert.Empty(solarsystems);

        var solarsystemjumps = await _services.GetSolarSystemJumpList();
        Assert.NotNull(solarsystemjumps);
        Assert.Empty(solarsystemjumps);


        bool importSuccess = await _services.Import();
        Assert.True(importSuccess);

        solarsystems = await _services.GetSolarSystemList();
        Assert.NotNull(solarsystems);
        Assert.NotEmpty(solarsystems);
        Assert.Equal(8036,solarsystems.Count()); //k-space+WH

        solarsystemjumps = await _services.GetSolarSystemJumpList();
        Assert.NotNull(solarsystemjumps);
        Assert.NotEmpty(solarsystemjumps);
        Assert.Equal(8036,solarsystemjumps.Count()); //k-space+WH
    }



    [Fact, Priority(4)]
    public async Task Search_System()
    {
        Assert.NotNull(_services);
        //TEST empty
        var empty_result = await _services.SearchSystem("");
        Assert.Null(empty_result);
        //TEST JITA
        var jita_result = await _services.SearchSystem(SOLAR_SYSTEM_JITA_NAME);
        Assert.NotNull(jita_result);
        Assert.Single(jita_result);

        //TEST JI for JITA partial
        var ji_result = await _services.SearchSystem(SOLAR_SYSTEM_JIT_NAME);
        Assert.NotNull(ji_result);
        Assert.Contains(ji_result, x => x.Name.Contains(SOLAR_SYSTEM_JITA_NAME, StringComparison.OrdinalIgnoreCase));


        //TEST AMARR
        var amarr_result = await _services.SearchSystem(SOLAR_SYSTEM_AMARR_NAME);
        Assert.NotNull(amarr_result);
        Assert.Single(amarr_result);

        //TEST AMA for AMARR partial
        var ama_result = await _services.SearchSystem(SOLAR_SYSTEM_AMA_NAME);
        Assert.NotNull(ama_result);
        Assert.Contains(ama_result, x => x.Name.Contains(SOLAR_SYSTEM_AMARR_NAME, StringComparison.OrdinalIgnoreCase));


        //TEST WH
        var wh_result = await _services.SearchSystem(SOLAR_SYSTEM_WH_NAME);
        Assert.NotNull(wh_result);
        Assert.Single(wh_result);

        var wh_partial_result = await _services.SearchSystem(SOLAR_SYSTEM_WH_PARTIAL_NAME);
        Assert.NotNull(wh_partial_result);
        Assert.Contains(wh_partial_result, x => x.Name.Contains(SOLAR_SYSTEM_WH_NAME, StringComparison.OrdinalIgnoreCase));
    }

    [Fact, Priority(5)]
    public async Task Search_System_By_Id()
    {
        Assert.NotNull(_services);
        //TEST bad value
        var bad_result = await _services.SearchSystemById(-1);
        Assert.Null(bad_result);

        //TEST empty
        var empty_result = await _services.SearchSystemById(0);
        Assert.Null(empty_result);
        //TEST JITA
        var jita_result = await _services.SearchSystemById(SOLAR_SYSTEM_JITA_ID);
        Assert.NotNull(jita_result);
        Assert.Equal(SOLAR_SYSTEM_JITA_ID,jita_result.SolarSystemID);

        //TEST HW
        var wh_result = await _services.SearchSystemById(31001123);
        Assert.NotNull(wh_result);
        Assert.Equal(31001123,wh_result.SolarSystemID);

    }
}


