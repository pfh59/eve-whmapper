using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHInstances;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHNotes;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Repositories.WHMapAccesses;

using Xunit.Priority;

namespace WHMapper.Tests.Services;


[Collection("C2-Services")]
public class DbIntegrationTest
{
    private const int EVE_CHARACTERE_ID = 2113720458;
    private const int EVE_CHARACTERE_ID2 = 2113932209;
    private const string EVE_CHARACTERE_NAME = "TOTO";
    private const string EVE_CHARACTERE_NAME2 = "TITI";

    private const int EVE_CORPO_ID = 1344654522;
    private const string EVE_CORPO_NAME = "Corp1";
    private const int EVE_CORPO_ID2 = 123456789;
    private const string EVE_CORPO_NAME2 = "Corp12";
    private const int EVE_ALLIANCE_ID = 1354830081;
        

    private const int FOOBAR_SYSTEM_ID = 123456;
    private const string FOOBAR ="FooBar";
    private const string FOOBAR2 = "FooBar2";
    private const string FOOBAR_UPDATED = "FooBar Updated";

    private const int FOOBAR_SYSTEM_ID2 = 1234567;
    private const int FOOBAR_SYSTEM_ID3 = 987456;
    private const string FOOBAR_SHORT_UPDATED = "FooBarU";
    private IDbContextFactory<WHMapperContext>? _contextFactory;



    public DbIntegrationTest()
    {
        //Create DB Context
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddDbContextFactory<WHMapperContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection")));

        var provider = services.BuildServiceProvider();
        if(provider!=null)
        {
            _contextFactory = provider.GetService<IDbContextFactory<WHMapperContext>>();
        }


    }



    [Fact, Priority(1)]
    public async Task DeleteAndCreateDatabse()
    {
        Assert.NotNull(_contextFactory);
        using (var context = _contextFactory.CreateDbContext())
        {
            //Delete all to make a fresh Db
            bool dbDeleted = await context.Database.EnsureDeletedAsync();
            Assert.True(dbDeleted);
            bool dbCreated = await context.Database.EnsureCreatedAsync();
            Assert.True(dbCreated);
        }
    }

    [Fact, Priority(2)]
    public async Task CRUD_WHMAP()
    {
        Assert.NotNull(_contextFactory);
        //Create IWHMapRepository
        IWHMapRepository repo = new WHMapRepository(new NullLogger<WHMapRepository>(),_contextFactory);


        var resEmpty = await repo.GetAll();
        Assert.NotNull(resEmpty);
        Assert.Empty(resEmpty);

        //ADD WHMAP
        var result1 = await repo.Create(new WHMap(FOOBAR));
        Assert.NotNull(result1);
        Assert.Equal(FOOBAR, result1?.Name);

        //ADD WHMAP
        var result2 = await repo.Create(new WHMap(FOOBAR2));
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR2, result2?.Name);

        //GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        //ADD test duplicate
        var resultDuplicate = await repo.Create(new WHMap(FOOBAR));
        Assert.Null(resultDuplicate);

        //GetALL
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);


        //GetById
        var resultById1 = await repo.GetById(1);
        Assert.NotNull(resultById1);
        Assert.Equal(1, resultById1.Id);

        var resultById2 = await repo.GetById(2);
        Assert.NotNull(resultById2);
        Assert.Equal(2, resultById2.Id);

        var resByBadId = await repo.GetById(10);
        Assert.Null(resByBadId);

        resByBadId = await repo.GetById(-10);
        Assert.Null(resByBadId);

        //getByName
        var resByName = await repo.GetByNameAsync(FOOBAR);
        Assert.NotNull(resByName);
        Assert.Equal(FOOBAR, resByName?.Name);

        //bad getByName
        var resBadName = await repo.GetByNameAsync(FOOBAR_UPDATED);
        Assert.Null(resBadName);
        

        //update
        resultById1.Name = FOOBAR_UPDATED;
        var resultUpdate1 = await repo.Update(resultById1.Id, resultById1);
        Assert.NotNull(resultUpdate1);
        Assert.Equal(FOOBAR_UPDATED, resultUpdate1.Name);

        //bad update
        var resultUpdateBad = await repo.Update(-10, resultById1);
        Assert.Null(resultUpdateBad);

        //update dupkicate
        resultById2.Name = FOOBAR_UPDATED;
        var resultUpdate2 = await repo.Update(resultById2.Id, resultById2);
        Assert.Null(resultUpdate2);


        //Delete WHMAP
        Assert.NotNull(result1);
        var resultDelete1 = await repo.DeleteById(result1.Id);
        Assert.True(resultDelete1);

        var resBadDelete = await repo.DeleteById(-10);
        Assert.False(resBadDelete);

        //delete all
        var resultDeleteAll = await repo.DeleteAll();
        Assert.True(resultDeleteAll);

        //nothing to delete all
        var resultDeleteAll2 = await repo.DeleteAll();
        Assert.False(resultDeleteAll2);
    }

    [Fact, Priority(3)]
    public async Task CRUD_WHSystem()
    {
        Assert.NotNull(_contextFactory);
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(new NullLogger<WHMapRepository>(),_contextFactory);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);

        //Create IWHSystemRepository
        IWHSystemRepository repo = new WHSystemRepository(new NullLogger<WHSystemRepository>(),_contextFactory);


        //GETALL system => return empty arry
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.Empty(results);

        //ADD WHSystem1
        Assert.NotNull(map);
        var result1 = await repo.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(result1);
        Assert.Equal(FOOBAR, result1?.Name);
        Assert.Equal(1, result1?.SecurityStatus);
        Assert.False(result1?.Locked);

        //ADD WHSystem2
        var result2 = await repo.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID2, FOOBAR2, 1));
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR2, result2?.Name);
        Assert.Equal(1, result2?.SecurityStatus);
        Assert.False(result2?.Locked);

        //GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        //add duplicate
        var resultDuplicate = await repo.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID2, FOOBAR2, 1));
        Assert.Null(resultDuplicate);

        //GetALL
        results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);

        //GetById
        Assert.NotNull(result1);
        var resultById= await repo.GetById(result1.Id);
        Assert.NotNull(resultById);
        Assert.Equal(FOOBAR_SYSTEM_ID, resultById.SoloarSystemId);
        Assert.Equal(FOOBAR, resultById.Name);
        Assert.Equal(1, resultById.SecurityStatus);

        var resBadById = await repo.GetById(-10);
        Assert.Null(resBadById);


        //update
        result1.Name = FOOBAR_UPDATED;
        result1.SecurityStatus = 0.5F;
        result1.AlternateName = FOOBAR_SHORT_UPDATED;
        var resultUpdate = await repo.Update(result1.Id, result1);
        Assert.NotNull(resultUpdate);
        Assert.Equal(FOOBAR_UPDATED, resultUpdate?.Name);
        Assert.Equal(0.5F, resultUpdate?.SecurityStatus);
        Assert.Equal(FOOBAR_SHORT_UPDATED, resultUpdate?.AlternateName);

        //update duplicate
        Assert.NotNull(result2);
        result2.Name = FOOBAR_UPDATED;
        var resultUpdateDuplicate = await repo.Update(result2.Id, result2);
        Assert.Null(resultUpdateDuplicate);


        //GetByName
        var resByName = await repo.GetByName(FOOBAR_UPDATED);
        Assert.NotNull(resByName);
        Assert.Equal(FOOBAR_UPDATED, resByName?.Name);
        Assert.Equal(0.5F, resByName?.SecurityStatus);

        //Delete WHSytem
        var resDel1 = await repo.DeleteById(result1.Id);
        Assert.True(resDel1);

        var resDel2 = await repo.DeleteById(result2.Id);
        Assert.True(resDel2);

        var resBadDel = await repo.DeleteById(-10);
        Assert.False(resBadDel);

        //Delete WHMAP
        var mapDeleted= await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);
    }

    [Fact, Priority(4)]
    public async Task CRUD_WHLink()
    {
        Assert.NotNull(_contextFactory);
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(new NullLogger<WHMapRepository>(),_contextFactory);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);


        //Create IWHSystemRepository
        IWHSystemRepository repoWH = new WHSystemRepository(new NullLogger<WHSystemRepository>(),_contextFactory);
        Assert.NotNull(map);
        var whSys1 = await repoWH.Create(new WHSystem(map.Id,FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(whSys1);
        Assert.Equal(FOOBAR_SYSTEM_ID, whSys1.SoloarSystemId);
        Assert.Equal(FOOBAR, whSys1.Name);
        Assert.Equal(1, whSys1.SecurityStatus);
        Assert.False(whSys1.Locked);

        var whSys2 = await repoWH.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID2, FOOBAR2, 'A', 1));
        Assert.NotNull(whSys2);
        Assert.Equal(FOOBAR_SYSTEM_ID2, whSys2.SoloarSystemId);
        Assert.Equal(FOOBAR2, whSys2.Name);
        Assert.Equal(1, whSys2.SecurityStatus);
        Assert.Equal(Convert.ToByte('A'), whSys2.NameExtension);
        Assert.False(whSys2.Locked);

        //GetCountAsync
        var count = await repoWH.GetCountAsync();
        Assert.Equal(2, count);

        var whSys3 = await repoWH.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID3, FOOBAR_SHORT_UPDATED, 'B', 1));
        Assert.NotNull(whSys3);
        Assert.Equal(FOOBAR_SYSTEM_ID3, whSys3.SoloarSystemId);
        Assert.Equal(FOOBAR_SHORT_UPDATED, whSys3.Name);
        Assert.Equal(1, whSys3.SecurityStatus);
        Assert.Equal(Convert.ToByte('B'), whSys3.NameExtension);
        Assert.False(whSys3.Locked);


        //Create IWHSystemLinkRepository
        IWHSystemLinkRepository repo = new WHSystemLinkRepository(new NullLogger<WHSystemLinkRepository>(),_contextFactory);


        var resultAllEmpty = await repo.GetAll();
        Assert.NotNull(resultAllEmpty);
        Assert.Empty(resultAllEmpty);

        //add whsystem link1
        var link1 = await repo.Create(new WHSystemLink(map.Id,whSys1.Id, whSys2.Id));
        Assert.NotNull(link1);
        Assert.Equal(whSys1.Id, link1.IdWHSystemFrom);
        Assert.Equal(whSys2.Id, link1.IdWHSystemTo);
        Assert.Equal(SystemLinkEolStatus.Normal, link1.EndOfLifeStatus);
        Assert.Equal(SystemLinkMassStatus.Normal, link1.MassStatus);
        Assert.Equal(SystemLinkSize.Large, link1.Size);

        //add whsystem link2
        var link2 = await repo.Create(new WHSystemLink(map.Id, whSys1.Id, whSys3.Id));
        Assert.NotNull(link2);
        Assert.Equal(whSys1.Id, link2.IdWHSystemFrom);
        Assert.Equal(whSys3.Id, link2.IdWHSystemTo);
        Assert.Equal(SystemLinkEolStatus.Normal, link2.EndOfLifeStatus);
        Assert.Equal(SystemLinkMassStatus.Normal, link2.MassStatus);
        Assert.Equal(SystemLinkSize.Large, link2.Size);

        //GetCountAsync
        var countLinks = await repo.GetCountAsync();
        Assert.Equal(2, countLinks);

        //add duplicate link
        var linkDuplicate = await repo.Create(new WHSystemLink(map.Id, whSys1.Id, whSys2.Id));
        Assert.Null(linkDuplicate);


        var resultAll = await repo.GetAll();
        Assert.NotNull(resultAll);
        Assert.NotEmpty(resultAll);

        var linkById = await repo.GetById(link1.Id);
        Assert.NotNull(linkById);
        Assert.Equal(linkById.Id, linkById.Id);

        var linkBadById = await repo.GetById(-10);
        Assert.Null(linkBadById);

        //update link1
        link1.EndOfLifeStatus = SystemLinkEolStatus.EOL4h;
        link1.MassStatus = SystemLinkMassStatus.Verge;
        var linkUpdate1 = await repo.Update(link1.Id, link1);
        Assert.NotNull(linkUpdate1);
        Assert.Equal(whSys1.Id, linkUpdate1.IdWHSystemFrom);
        Assert.Equal(whSys2.Id, linkUpdate1.IdWHSystemTo);
        Assert.Equal(SystemLinkEolStatus.EOL4h, linkUpdate1.EndOfLifeStatus);
        Assert.Equal(SystemLinkMassStatus.Verge, linkUpdate1.MassStatus);
        Assert.Equal(SystemLinkSize.Large, linkUpdate1.Size);

        //update dupkicate
        link2.IdWHSystemTo = linkUpdate1.IdWHSystemTo;
        var linkUpdateDuplicate = await repo.Update(link2.Id, link2);
        Assert.Null(linkUpdateDuplicate);

        //Delete link
        var linkDel1 = await repo.DeleteById(link1.Id);
        Assert.True(linkDel1);
        var linkDel2 = await repo.DeleteById(link2.Id);
        Assert.True(linkDel2);

        var linkBadDel = await repo.DeleteById(-10);
        Assert.False(linkBadDel);


        //Delete WHMAP
        var mapDeleted = await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);

        var links = repo.GetAll();
        Assert.NotNull(links);
    }

    [Fact, Priority(5)]
    public async Task CRUD_WHSignature()
    {
        Assert.NotNull(_contextFactory);
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(new NullLogger<WHMapRepository>(),_contextFactory);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);


        //Create IWHMapRepository
        IWHSystemRepository repoWH = new WHSystemRepository(new NullLogger<WHSystemRepository>(),_contextFactory);
        Assert.NotNull(map);
        var whSys1 = await repoWH.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(whSys1);
        Assert.Equal(FOOBAR_SYSTEM_ID, whSys1.SoloarSystemId);
        Assert.Equal(FOOBAR, whSys1.Name);
        Assert.Equal(1, whSys1.SecurityStatus);
        Assert.False(whSys1.Locked);


        //Create IWHMapRepository
        IWHSignatureRepository repo = new WHSignatureRepository(new NullLogger<WHSignatureRepository>(),_contextFactory);

        //get all empty
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.Empty(results);

        //ADD WHSignature
        var result1 = await repo.Create(new WHSignature(whSys1.Id,FOOBAR));
        Assert.NotNull(result1);
        Assert.Equal(whSys1.Id, result1.WHId);
        Assert.Equal(FOOBAR, result1?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result1?.Group);

        var result2 = await repo.Create(new WHSignature(whSys1.Id, FOOBAR2));
        Assert.NotNull(result2);
        Assert.Equal(whSys1.Id, result2.WHId);
        Assert.Equal(FOOBAR2, result2?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result2?.Group);

        //GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        var resDuplicate = await repo.Create(new WHSignature(whSys1.Id, FOOBAR2));
        Assert.Null(resDuplicate);

        //GetALL
        results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);


        //GetById
        var resById1 = await repo.GetById(1);
        Assert.NotNull(resById1);
        Assert.Equal(FOOBAR, resById1?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, resById1?.Group);

        var resById2 = await repo.GetById(2);
        Assert.NotNull(resById2);
        Assert.Equal(FOOBAR2, resById2?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, resById2?.Group);

        var resBadbyId = await repo.GetById(-10);
        Assert.Null(resBadbyId);

        //GetByWHId
        var resByWHId = await repo.GetByWHId(whSys1.Id);
        Assert.NotNull(resByWHId);
        Assert.NotEmpty(resByWHId);

        var resBadByWHId = await repo.GetByWHId(-10);
        Assert.NotNull(resBadByWHId);
        Assert.Empty(resBadByWHId);

        //GetByName
        var resByName1 = await repo.GetByName(FOOBAR);
        Assert.NotNull(resByName1);
        Assert.Equal(FOOBAR, resByName1.Name);
        Assert.Equal(WHSignatureGroup.Unknow, resByName1.Group);

        //update, name characters max 7
        Assert.NotNull(resById1);
        resById1.Name = FOOBAR_SHORT_UPDATED;
        resById1.Group = WHSignatureGroup.Wormhole;
        var resUpdate = await repo.Update(resById1.Id, resById1);
        Assert.NotNull(resUpdate);
        Assert.Equal(FOOBAR_SHORT_UPDATED, resUpdate.Name);
        Assert.Equal(WHSignatureGroup.Wormhole, resUpdate.Group);

        //update duplicate
        Assert.NotNull(resById2);
        resById2.Name = FOOBAR_SHORT_UPDATED;
        resById2.Group = WHSignatureGroup.Wormhole;
        var resUpdateDuplicate = await repo.Update(resById2.Id, resById2);
        Assert.Null(resUpdateDuplicate);

        //updates ienumerable
        Assert.NotNull(results);
        (results.ToArray())[0].UpdatedBy = FOOBAR;
        var resultsUpdates = await repo.Update(results);
        Assert.NotNull(resultsUpdates);
        Assert.Contains(resultsUpdates, x => x?.UpdatedBy == FOOBAR);

        //delete
        var resDel1 = await repo.DeleteById(resById1.Id);
        Assert.True(resDel1);

        var resBadDelete = await repo.DeleteById(-10);
        Assert.False(resBadDelete);


        //remove ALL WHSystemSignature
        var badDelete = await repo.DeleteByWHId(123);//test bad system id return false
        Assert.False(badDelete);

        Assert.NotNull(result2);
        var deleteStatus=await repo.DeleteByWHId(whSys1.Id);//test good system but empty sig
        Assert.True(deleteStatus);

       //Add multi sig
       IList<WHSignature> sigs = new List<WHSignature>();
       sigs.Add(new WHSignature(whSys1.Id, FOOBAR));
       sigs.Add(new WHSignature(whSys1.Id,FOOBAR_SHORT_UPDATED));


       var resultWHSigs = await repo.Create(sigs);
       Assert.NotNull(resultWHSigs);
       Assert.Equal(2, resultWHSigs.Count());

        //delete
        deleteStatus = await repo.DeleteByWHId(whSys1.Id);//test good system but empty sig
        Assert.True(deleteStatus);

        deleteStatus = await repo.DeleteByWHId(-10);//test good system but empty sig
        Assert.False(deleteStatus);

        //Delete WHMAP
        var mapDeleted = await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);

    }

    [Fact, Priority(6)]
    public async Task CRUD_WHNote()
    {
        Assert.NotNull(_contextFactory);
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(new NullLogger<WHMapRepository>(),_contextFactory);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);

        //Create AccessRepo
        IWHNoteRepository repo = new WHNoteRepository(new NullLogger<WHNoteRepository>(), _contextFactory);

        //get ALL empty
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.Empty(results);

        //ADD Note1
        Assert.NotNull(map);
        var result1 = await repo.Create(new WHNote(map.Id,FOOBAR_SYSTEM_ID,FOOBAR));
        Assert.NotNull(result1);
        Assert.Equal(FOOBAR_SYSTEM_ID, result1.SoloarSystemId);
        Assert.Equal(FOOBAR, result1.Comment);
        Assert.Equal(WHSystemStatus.Unknown, result1.SystemStatus);

        //ADD Note2
        var result2 = await repo.Create(new WHNote(map.Id,FOOBAR_SYSTEM_ID2,WHSystemStatus.Hostile));
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_SYSTEM_ID2, result2.SoloarSystemId);
        Assert.Equal(string.Empty, result2.Comment);
        Assert.Equal(WHSystemStatus.Hostile, result2.SystemStatus);
        
        //GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        //ADD Access duplicate
        var resultDuplicate = await repo.Create(new WHNote(map.Id,FOOBAR_SYSTEM_ID2, FOOBAR));
        Assert.Null(resultDuplicate);

        //GetALL
        results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);


        //GetbyID
        var resultById = await repo.GetById(1);
        Assert.NotNull(resultById);
        Assert.Equal(FOOBAR_SYSTEM_ID, resultById.SoloarSystemId);
        Assert.Equal(FOOBAR, resultById.Comment);

        var resultBadById = await repo.GetById(-10);
        Assert.Null(resultBadById);

        //Get
        var resultBySolarSystemId = await repo.Get(map.Id,FOOBAR_SYSTEM_ID);
        Assert.NotNull(resultBySolarSystemId);
        Assert.Equal(FOOBAR_SYSTEM_ID, resultBySolarSystemId.SoloarSystemId);
        Assert.Equal(FOOBAR, resultBySolarSystemId.Comment);

        var resultBadBySolarSystemId = await repo.Get(map.Id,-10);
        Assert.Null(resultBadBySolarSystemId);

        //update
        result1.Comment = FOOBAR_SHORT_UPDATED;
        var resultUpdate1 = await repo.Update(result1.Id, result1);
        Assert.NotNull(resultUpdate1);
        Assert.Equal(FOOBAR_SHORT_UPDATED, resultUpdate1.Comment);

        //duplicate update
        result2.SoloarSystemId = FOOBAR_SYSTEM_ID;
        var resultUpdate2 = await repo.Update(result2.Id, result2);
        Assert.Null(resultUpdate2);

        //Delete
        var resultdel1 = await repo.DeleteById(result1.Id);
        Assert.True(resultdel1);

        var resultdel2 = await repo.DeleteById(result2.Id);
        Assert.True(resultdel2);

        var resultBaddel = await repo.DeleteById(-10);
        Assert.False(resultBaddel);

        //null update
        var resultUpdateNull = await repo.Update(-10, null!);
        Assert.Null(resultUpdateNull);

        //bad id update
        var resultUpdateBadId = await repo.Update(-10, result1);
        Assert.Null(resultUpdateBadId);

        //Delete WHMAP
        var mapDeleted = await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);
    }

        [Fact, Priority(7)]
    public async Task CRUD_WHMapAccess()
    {
        Assert.NotNull(_contextFactory);

        // Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(new NullLogger<WHMapRepository>(), _contextFactory);

        // Add WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);

        // Create WHMapAccessRepository
        IWHMapAccessRepository repo = new WHMapAccessRepository(new NullLogger<WHMapAccessRepository>(), _contextFactory);

        // GetAll should be empty
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.Empty(results);

        // Add access
        var access1 = await repo.Create(new WHMapAccess(map.Id, EVE_CHARACTERE_ID, EVE_CHARACTERE_NAME, WHAccessEntity.Character));
        Assert.NotNull(access1);
        Assert.Equal(map.Id, access1.WHMapId);
        Assert.Equal(EVE_CHARACTERE_ID, access1.EveEntityId);
        Assert.Equal(EVE_CHARACTERE_NAME, access1.EveEntityName);


        // Add another access
        var access2 = await repo.Create(new WHMapAccess(map.Id, EVE_CHARACTERE_ID2, EVE_CHARACTERE_NAME2, WHAccessEntity.Character));
        Assert.NotNull(access2);

        // GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        // Duplicate add
        var duplicate = await repo.Create(new WHMapAccess(map.Id, EVE_CHARACTERE_ID, EVE_CHARACTERE_NAME, WHAccessEntity.Character));
        Assert.Null(duplicate);

        // GetAll
        results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);

        // GetById
        var byId = await repo.GetById(access1.Id);
        Assert.NotNull(byId);
        Assert.Equal(EVE_CHARACTERE_ID, byId.EveEntityId);

        var badById = await repo.GetById(-10);
        Assert.Null(badById);

        // Update
        access1.EveEntityName = "UPDATED";
        var updated = await repo.Update(access1.Id, access1);
        Assert.NotNull(updated);
        Assert.Equal("UPDATED", updated.EveEntityName);

        // Duplicate update
        access2.EveEntityId = EVE_CHARACTERE_ID;
        var updateDuplicate = await repo.Update(access2.Id, access2);
        Assert.Null(updateDuplicate);

        // Delete
        var del1 = await repo.DeleteById(access1.Id);
        Assert.True(del1);

        var del2 = await repo.DeleteById(access2.Id);
        Assert.True(del2);

        var badDel = await repo.DeleteById(-10);
        Assert.False(badDel);

        // Delete WHMAP
        var mapDeleted = await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);
    }


    [Fact, Priority(8)]
    public async Task CRUD_WHInstance()
    {
        Assert.NotNull(_contextFactory);

        // Create repository
        IWHInstanceRepository repo = new WHInstanceRepository(new NullLogger<WHInstanceRepository>(), _contextFactory);

        // GetAll should be empty
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.Empty(results);

        // Add instance
        var instance1 = await repo.Create(new WHInstance(
            "Instance1",           // name
            1,                     // ownerEveEntityId
            "OwnerName1",          // ownerEveEntityName
            WHAccessEntity.Character, // ownerEveEntityType
            1,                  // creatorCharacterId
            "creatorCharacterName1" //creatorCharacterName
            , "Description1"         // description
        ));
        Assert.NotNull(instance1);
        Assert.Equal("Instance1", instance1.Name);
        Assert.Equal(1, instance1.OwnerEveEntityId);
        Assert.Equal("OwnerName1", instance1.OwnerEveEntityName);
        Assert.Equal(WHAccessEntity.Character, instance1.OwnerType);
        Assert.Equal(1, instance1.CreatorCharacterId);
        Assert.Equal("creatorCharacterName1", instance1.CreatorCharacterName);
        Assert.Equal("Description1", instance1.Description);

        // Add another instance
        var instance2 = await repo.Create(new WHInstance(
            "Instance2",
            2,
            "OwnerName2",
            WHAccessEntity.Corporation,
            1,
            "Description2"
        ));
        Assert.NotNull(instance2);

        // GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        // Duplicate add (same OwnerEveEntityId and OwnerType)
        var duplicate = await repo.Create(new WHInstance(
            "AnotherName",           // name can be different
            1,                       // ownerEveEntityId
            "OtherOwnerName",        // ownerEveEntityName
            WHAccessEntity.Character, // ownerEveEntityType
            1,                       // creatorCharacterId
            "creatorCharacterName1", // creatorCharacterName
            "OtherDescription"
        ));
        Assert.Null(duplicate);

        // GetAll
        results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);

        // GetById
        var byId = await repo.GetById(instance1.Id);
        Assert.NotNull(byId);
        Assert.Equal("Instance1", byId.Name);

        var badById = await repo.GetById(-10);
        Assert.Null(badById);

        // Update
        instance1.Description = "UPDATED";
        var updated = await repo.Update(instance1.Id, instance1);
        Assert.NotNull(updated);
        Assert.Equal("UPDATED", updated.Description);

        // Duplicate update (same OwnerEveEntityId and OwnerType)
        instance2.OwnerEveEntityId = 1;
        instance2.OwnerType = WHAccessEntity.Character;
        var updateDuplicate = await repo.Update(instance2.Id, instance2);
        Assert.Null(updateDuplicate);

        // Null update
        var nullUpdate = await repo.Update(-10, null!);
        Assert.Null(nullUpdate);

        // Bad id update
        var badIdUpdate = await repo.Update(-10, instance1);
        Assert.Null(badIdUpdate);

        // Delete
        var del1 = await repo.DeleteById(instance1.Id);
        Assert.True(del1);

        var del2 = await repo.DeleteById(instance2.Id);
        Assert.True(del2);

        var badDel = await repo.DeleteById(-10);
        Assert.False(badDel);

        // Try to get deleted instance
        var deletedInstance = await repo.GetById(instance1.Id);
        Assert.Null(deletedInstance);

        // Try to delete again
        var delAgain = await repo.DeleteById(instance1.Id);
        Assert.False(delAgain);
    }

    [Fact, Priority(9)]
    public async Task CRUD_WHRoute()
    {
        Assert.NotNull(_contextFactory);
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(new NullLogger<WHMapRepository>(),_contextFactory);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);

        //Create AccessRepo
        IWHRouteRepository repo = new WHRouteRepository(new NullLogger<WHRouteRepository>(), _contextFactory);

        //get ALL empty
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.Empty(results);

        //ADD Route1
        Assert.NotNull(map);
        var result1 = await repo.Create(new WHRoute(map.Id,FOOBAR_SYSTEM_ID));
        Assert.NotNull(result1);
        Assert.Equal(FOOBAR_SYSTEM_ID, result1.SolarSystemId);
        Assert.Null(result1.EveEntityId);

        //ADD Route2
        var result2 = await repo.Create(new WHRoute(map.Id,FOOBAR_SYSTEM_ID2, EVE_CORPO_ID));
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_SYSTEM_ID2, result2.SolarSystemId);
        Assert.Equal(EVE_CORPO_ID, result2.EveEntityId);

        //GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        //ADD Route duplicate
        var resultDuplicate = await repo.Create(new WHRoute(map.Id,FOOBAR_SYSTEM_ID2, EVE_CORPO_ID));
        Assert.Null(resultDuplicate);

        //GetALL
        results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);

        //GetbyID
        var resultById = await repo.GetById(1);
        Assert.NotNull(resultById);
        Assert.Equal(FOOBAR_SYSTEM_ID, resultById.SolarSystemId);

        var resultBadById = await repo.GetById(-10);
        Assert.Null(resultBadById);

        //update
        result1.EveEntityId = EVE_CHARACTERE_ID;
        var resultUpdate1 = await repo.Update(result1.Id, result1);
        Assert.NotNull(resultUpdate1);
        Assert.Equal(EVE_CHARACTERE_ID, result1.EveEntityId );

        //duplicate update
        result2.SolarSystemId = FOOBAR_SYSTEM_ID;
        result2.EveEntityId = EVE_CHARACTERE_ID;
        var resultUpdate2 = await repo.Update(result2.Id, result2);
        Assert.Null(resultUpdate2);

        //Delete
        var resultdel1 = await repo.DeleteById(result1.Id);
        Assert.True(resultdel1);

        var resultdel2 = await repo.DeleteById(result2.Id);
        Assert.True(resultdel2);

        var resultBaddel = await repo.DeleteById(-10);
        Assert.False(resultBaddel);

        //ADD Route1 with eveentityid
        var result = await repo.Create(new WHRoute(map.Id,FOOBAR_SYSTEM_ID, EVE_CHARACTERE_ID));
        Assert.NotNull(result);
        Assert.Equal(FOOBAR_SYSTEM_ID, result1.SolarSystemId);
        Assert.Equal(EVE_CHARACTERE_ID,result1.EveEntityId);

        //get by eveentityid
        var resultByEveEntityId = await repo.GetRoutesByEveEntityId(map.Id,EVE_CHARACTERE_ID);
        Assert.NotNull(resultByEveEntityId);
        Assert.NotEmpty(resultByEveEntityId);

        //null update
        var resultUpdateNull = await repo.Update(-10, null!);
        Assert.Null(resultUpdateNull);

        //bad id update
        var resultUpdateBadId = await repo.Update(-10, result1);
        Assert.Null(resultUpdateBadId);

        //Delete
        var resultdel = await repo.DeleteById(result.Id);
        Assert.True(resultdel);

        //Delete WHMAP
        var mapDeleted = await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);

    }

    [Fact, Priority(10)]
    public async Task CRUD_WHJumpLog()
    {
        Assert.NotNull(_contextFactory);

        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(new NullLogger<WHMapRepository>(),_contextFactory);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);


        //Init two system IWHSystemRepository
        IWHSystemRepository repoWH = new WHSystemRepository(new NullLogger<WHSystemRepository>(),_contextFactory);
        Assert.NotNull(map);
        var whSys1 = await repoWH.Create(new WHSystem(map.Id,FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(whSys1);
        var whSys2 = await repoWH.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID2, FOOBAR2, 'A', 1));
        Assert.NotNull(whSys2);

        //Init link 
        IWHSystemLinkRepository repoLink = new WHSystemLinkRepository(new NullLogger<WHSystemLinkRepository>(),_contextFactory);

        //add whsystem link1
        var link1 = await repoLink.Create(new WHSystemLink(map.Id,whSys1.Id, whSys2.Id));
        Assert.NotNull(link1);

        //Create JumlLogRepo
        IWHJumpLogRepository repo = new WHJumpLogRepository(new NullLogger<WHJumpLogRepository>(), _contextFactory);

        //ADD JumpLog lmanuel 
        var result1 = await repo.Create(new WHJumpLog(link1.Id,EVE_CHARACTERE_ID));
        Assert.NotNull(result1);
        Assert.Equal(EVE_CHARACTERE_ID, result1.CharacterId);
        Assert.Null(result1.ShipTypeId);
        Assert.Null(result1.ShipItemId);
        Assert.Null(result1.ShipMass);

        //ADD JumpLog2 
        var result2 = await repo.Create(new WHJumpLog(link1.Id,EVE_CHARACTERE_ID2,54731,1037556721774,300000000));
        Assert.NotNull(result2);
        Assert.Equal(EVE_CHARACTERE_ID2, result2.CharacterId);
        Assert.Equal(54731, result2.ShipTypeId);
        Assert.Equal(1037556721774, result2.ShipItemId);
        Assert.Equal(300000000, result2.ShipMass);

        //GetCountAsync
        var count = await repo.GetCountAsync();
        Assert.Equal(2, count);

        //GetALL
        var results = await repo.GetAll();
        Assert.NotNull(results);
        Assert.NotEmpty(results);
        Assert.Equal(2, results.Count());

        //GetbyID
        var resultById = await repo.GetById(1);
        Assert.NotNull(resultById);
        Assert.Equal(EVE_CHARACTERE_ID, resultById.CharacterId);

        var resultBadById = await repo.GetById(-10);
        Assert.Null(resultBadById);

        //update
        result1.ShipMass = 100000000;
        var resultUpdate1 = await repo.Update(result1.Id, result1);
        Assert.NotNull(resultUpdate1);
        Assert.Equal(100000000, resultUpdate1.ShipMass);

        var badUpdate = await repo.Update(result2.Id, null!);
        Assert.Null(badUpdate);

        var badUpdate2 = await repo.Update(-12, result2);
        Assert.Null(badUpdate2);

        //duplicate update
        result2.CharacterId = EVE_CHARACTERE_ID;
        result2.JumpDate = result1.JumpDate;
        var resultUpdate2 = await repo.Update(result2.Id, result2);
        Assert.Null(resultUpdate2);

        //Delete
        var resultdel1 = await repo.DeleteById(result1.Id);
        Assert.True(resultdel1);

        var resultdel2 = await repo.DeleteById(result2.Id);
        Assert.True(resultdel2);

        var resultBaddel = await repo.DeleteById(-10);
        Assert.False(resultBaddel);

        //clean map
        var mapDeleted = await repoMap.DeleteById(map.Id);
    }
}
