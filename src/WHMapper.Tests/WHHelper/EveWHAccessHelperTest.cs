using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHMapAccesses;
using WHMapper.Repositories.WHInstances;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;

[Collection("C5-WHHelper")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class EveWHAccessHelperTest
{
    private int EVE_CHARACTERE_ID = 2113720458;
    private int EVE_CHARACTERE_ID2 = 153110579;

    private int EVE_CORPO_ID = 1344654522;
    private int EVE_ALLIANCE_ID = 1354830081;

    IDbContextFactory<WHMapperContext>? _contextFactory;
    private IEveMapperAccessHelper? _accessHelper;
    private IWHMapRepository? _whMapRepository;
    private IWHInstanceRepository? _whInstanceRepository;
    private IWHMapAccessRepository? _whMapAccessRepository;

    

    public EveWHAccessHelperTest()
    {

        //Create DB Context
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddDbContextFactory<WHMapperContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection")));

        services.AddHttpClient();


        var provider = services.BuildServiceProvider();
        if (provider != null)
        {
            var httpclientfactory = provider.GetService<IHttpClientFactory>();
            _contextFactory = provider.GetService<IDbContextFactory<WHMapperContext>>();
            if (_contextFactory != null && httpclientfactory != null)
            {
                _whMapRepository = new WHMapRepository(new NullLogger<WHMapRepository>(), _contextFactory);
                _whInstanceRepository = new WHInstanceRepository(new NullLogger<WHInstanceRepository>(), _contextFactory);
                _whMapAccessRepository = new WHMapAccessRepository(new NullLogger<WHMapAccessRepository>(), _contextFactory);

                var httpClient = httpclientfactory.CreateClient();
                httpClient.BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl);

                _accessHelper = new EveMapperAccessHelper(_whMapRepository, _whInstanceRepository, _whMapAccessRepository, new CharacterServices(httpClient));
            }
        }

        
    }

    [Fact, Priority(1)]
    public async Task Delete_And_Create_DB()
    {
        Assert.NotNull(_contextFactory);
        //Delete all to make a fresh Db

        using (var context = _contextFactory.CreateDbContext())
        {
            bool dbDeleted = await context.Database.EnsureDeletedAsync();
            Assert.True(dbDeleted);
            bool dbCreated = await context.Database.EnsureCreatedAsync();
            Assert.True(dbCreated);
        }

    }

    [Fact, Priority(2)]
    public async Task Eve_Mapper_No_Instance_No_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whInstanceRepository);

        // Without any instance, user should NOT have access
        var noAccess = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.False(noAccess);
    }

    [Fact, Priority(3)]
    public async Task Eve_Mapper_Instance_Character_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whInstanceRepository);

        // Create an instance owned by character
        var instance = new WHInstance("Test Instance", EVE_CHARACTERE_ID, "Test Character", WHAccessEntity.Character, EVE_CHARACTERE_ID, "Test Character");
        var createdInstance = await _whInstanceRepository.Create(instance);
        Assert.NotNull(createdInstance);

        // Add the owner as admin
        await _whInstanceRepository.AddInstanceAdminAsync(createdInstance.Id, EVE_CHARACTERE_ID, "Test Character", true);

        // Add character access
        var access = new WHInstanceAccess(createdInstance.Id, EVE_CHARACTERE_ID, "Test Character", WHAccessEntity.Character);
        await _whInstanceRepository.AddInstanceAccessAsync(access);

        // User should have access
        var hasAccess = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.True(hasAccess);

        // Other user should NOT have access
        var noAccess = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.False(noAccess);

        // Cleanup
        await _whInstanceRepository.DeleteById(createdInstance.Id);
    }

    [Fact, Priority(4)]
    public async Task Eve_Mapper_Instance_Corporation_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whInstanceRepository);

        // Create an instance owned by corporation
        var instance = new WHInstance("Corp Instance", EVE_CORPO_ID, "Test Corp", WHAccessEntity.Corporation, EVE_CHARACTERE_ID, "Test Character");
        var createdInstance = await _whInstanceRepository.Create(instance);
        Assert.NotNull(createdInstance);

        // Add the creator as admin
        await _whInstanceRepository.AddInstanceAdminAsync(createdInstance.Id, EVE_CHARACTERE_ID, "Test Character", true);

        // Add corporation access
        var access = new WHInstanceAccess(createdInstance.Id, EVE_CORPO_ID, "Test Corp", WHAccessEntity.Corporation);
        await _whInstanceRepository.AddInstanceAccessAsync(access);

        // User in corp should have access
        var hasAccess = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.True(hasAccess);

        // User NOT in corp should NOT have access
        var noAccess = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.False(noAccess);

        // Cleanup
        await _whInstanceRepository.DeleteById(createdInstance.Id);
    }

    [Fact, Priority(5)]
    public async Task Eve_Mapper_Instance_Admin_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whInstanceRepository);

        // Create an instance
        var instance = new WHInstance("Admin Instance", EVE_CHARACTERE_ID, "Test Character", WHAccessEntity.Character, EVE_CHARACTERE_ID, "Test Character");
        var createdInstance = await _whInstanceRepository.Create(instance);
        Assert.NotNull(createdInstance);

        // Add the owner as admin
        await _whInstanceRepository.AddInstanceAdminAsync(createdInstance.Id, EVE_CHARACTERE_ID, "Test Character", true);

        // Owner should be admin
        var isAdmin = await _accessHelper.IsEveMapperAdminAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.True(isAdmin);

        // Other user should NOT be admin
        var notAdmin = await _accessHelper.IsEveMapperAdminAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.False(notAdmin);

        // Add second user as admin
        await _whInstanceRepository.AddInstanceAdminAsync(createdInstance.Id, EVE_CHARACTERE_ID2, "Test Character 2", false);

        // Second user should now be admin
        var nowAdmin = await _accessHelper.IsEveMapperAdminAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.True(nowAdmin);

        // Cleanup
        await _whInstanceRepository.DeleteById(createdInstance.Id);
    }

    [Fact, Priority(6)]
    public async Task Eve_Mapper_Map_Instance_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whMapRepository);
        Assert.NotNull(_whInstanceRepository);

        // Create an instance
        var instance = new WHInstance("Map Instance", EVE_CHARACTERE_ID, "Test Character", WHAccessEntity.Character, EVE_CHARACTERE_ID, "Test Character");
        var createdInstance = await _whInstanceRepository.Create(instance);
        Assert.NotNull(createdInstance);

        // Add the owner as admin and access
        await _whInstanceRepository.AddInstanceAdminAsync(createdInstance.Id, EVE_CHARACTERE_ID, "Test Character", true);
        var access = new WHInstanceAccess(createdInstance.Id, EVE_CHARACTERE_ID, "Test Character", WHAccessEntity.Character);
        await _whInstanceRepository.AddInstanceAccessAsync(access);

        // Create a map in this instance
        var map = await _whMapRepository.Create(new WHMap("MAP", createdInstance.Id));
        Assert.NotNull(map);
        Assert.Equal("MAP", map.Name);
        Assert.Equal(createdInstance.Id, map.WHInstanceId);

        // User with instance access should have map access
        var hasAccess = await _accessHelper.IsEveMapperMapAccessAuthorized(EVE_CHARACTERE_ID, map.Id);
        Assert.True(hasAccess);

        // User without instance access should NOT have map access
        var noAccess = await _accessHelper.IsEveMapperMapAccessAuthorized(EVE_CHARACTERE_ID2, map.Id);
        Assert.False(noAccess);

        // Cleanup
        await _whMapRepository.DeleteById(map.Id);
        await _whInstanceRepository.DeleteById(createdInstance.Id);
    }
}


