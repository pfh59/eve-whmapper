﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHAdmins;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveMapper;
using Xunit.Priority;

namespace WHMapper.Tests.WHHelper;

[Collection("C5-WHHelper")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class EveWHAccessHelperTest
{
    private int EVE_CHARACTERE_ID = 2113720458;
    private int EVE_CHARACTERE_ID2 = 2113932209;

    private int EVE_CORPO_ID = 1344654522;
    private int EVE_ALLIANCE_ID = 1354830081;

    IDbContextFactory<WHMapperContext>? _contextFactory;
    private IEveMapperAccessHelper? _accessHelper;
    private IWHAccessRepository? _whAccessRepository;
    private IWHAdminRepository? _whAdminRepository;
    private IWHMapRepository? _whMapRepository;

    

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
                _whAccessRepository = new WHAccessRepository(new NullLogger<WHAccessRepository>(), _contextFactory);
                _whAdminRepository = new WHAdminRepository(new NullLogger<WHAdminRepository>(), _contextFactory);
                _whMapRepository = new WHMapRepository(new NullLogger<WHMapRepository>(), _contextFactory);
                _accessHelper = new EveMapperAccessHelper(_whAccessRepository, _whAdminRepository,_whMapRepository, new CharacterServices(httpclientfactory.CreateClient()));
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
    public async Task Eve_Mapper_User_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whAccessRepository);

        var fullAuthorize = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.True(fullAuthorize);

        var character = await _whAccessRepository.Create(new WHAccess(EVE_CHARACTERE_ID2, "TOTO",WHAccessEntity.Character));
        Assert.NotNull(character);
        Assert.Equal(EVE_CHARACTERE_ID2, character.EveEntityId);

        var notAuthorize = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.False(notAuthorize);

        var authorize = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.True(authorize);

        await _whAccessRepository.DeleteById(character.Id);

    }

    [Fact, Priority(3)]
    public async Task Eve_Mapper_Corpo_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whAccessRepository);

        var corpo = await _whAccessRepository.Create(new WHAccess(EVE_CORPO_ID, "TOTO",WHAccessEntity.Corporation));
        Assert.NotNull(corpo);
        Assert.Equal(EVE_CORPO_ID, corpo.EveEntityId);

        var authorizeU1 = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.True(authorizeU1);

        var authorizeU2 = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.False(authorizeU2);

        await _whAccessRepository.DeleteById(corpo.Id);
    }

    [Fact, Priority(4)]
    public async Task Eve_Mapper_Alliance_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whAccessRepository);

        var alliance = await _whAccessRepository.Create(new WHAccess(EVE_ALLIANCE_ID, "TOTO",WHAccessEntity.Alliance));
        Assert.NotNull(alliance);
        Assert.Equal(EVE_ALLIANCE_ID, alliance.EveEntityId);

        var authorizeU1 = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.True(authorizeU1);

        var authorizeU2 = await _accessHelper.IsEveMapperUserAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.True(authorizeU2);

        await _whAccessRepository.DeleteById(alliance.Id);
    }

    [Fact, Priority(5)]
    public async Task Eve_Mapper_Admin_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whAdminRepository);

        var fullAuthorize = await _accessHelper.IsEveMapperAdminAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.True(fullAuthorize);

        var character = await _whAdminRepository.Create(new WHAdmin(EVE_CHARACTERE_ID2, "TOTO"));
        Assert.NotNull(character);
        Assert.Equal(EVE_CHARACTERE_ID2, character.EveCharacterId);

        var notAuthorize = await _accessHelper.IsEveMapperAdminAccessAuthorized(EVE_CHARACTERE_ID);
        Assert.False(notAuthorize);

        var authorize = await _accessHelper.IsEveMapperAdminAccessAuthorized(EVE_CHARACTERE_ID2);
        Assert.True(authorize);

        await _whAdminRepository.DeleteById(character.Id);
    }

    [Fact, Priority(6)]
    public async Task Eve_Mapper_Map_Access()
    {
        Assert.NotNull(_accessHelper);
        Assert.NotNull(_whMapRepository);
        Assert.NotNull(_whAccessRepository);

        
        var map = await _whMapRepository.Create(new WHMap("MAP"));
        Assert.NotNull(map);
        Assert.Equal("MAP", map.Name);
        Assert.Empty(map.WHAccesses);

        
        var fullAuthorize = await _accessHelper.IsEveMapperMapAccessAuthorized(EVE_CHARACTERE_ID,map.Id);
        Assert.True(fullAuthorize);

        //add access to evemap lock all maps
        var characterAccess = await _whAccessRepository.Create(new WHAccess(EVE_CHARACTERE_ID, "TOTO",WHAccessEntity.Character));
        Assert.NotNull(characterAccess);
        Assert.Equal(EVE_CHARACTERE_ID, characterAccess.EveEntityId);

        var notAuthorize1 = await _accessHelper.IsEveMapperMapAccessAuthorized(EVE_CHARACTERE_ID,map.Id);
        Assert.False(notAuthorize1);

        var notAuthorize2 = await _accessHelper.IsEveMapperMapAccessAuthorized(EVE_CHARACTERE_ID2,map.Id);
        Assert.False(notAuthorize2);

        //add access to evemap only for user1
        map.WHAccesses.Add(characterAccess);
        await _whMapRepository.Update(map.Id,map);

        var authorize1 = await _accessHelper.IsEveMapperMapAccessAuthorized(EVE_CHARACTERE_ID,map.Id);
        Assert.True(authorize1);

        var notAuthorize2_2 = await _accessHelper.IsEveMapperMapAccessAuthorized(EVE_CHARACTERE_ID2,map.Id);
        Assert.False(notAuthorize2_2);

        await _whMapRepository.DeleteById(map.Id);
    }


}


