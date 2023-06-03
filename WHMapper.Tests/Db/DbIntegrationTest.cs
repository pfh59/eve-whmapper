using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHSystems;
using WHMapper.Tests.Attributes;
using Xunit;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Tests.Db;

[TestCaseOrderer("WHMapper.Tests.Orderers.PriorityOrderer", "WHMapper.Tests.Db")]
public class DbIntegrationTest
{
    private const int FOOBAR_SYSTEM_ID = 123456;
    private const string FOOBAR ="FooBar";
    private const string FOOBAR_UPDATED = "FooBar Updated";

    private const int FOOBAR_SYSTEM_ID2 = 1234567;
    private const string FOOBAR_SHORT_UPDATED = "FooBarU";
    private WHMapperContext _context;

    
    public DbIntegrationTest()
    {
        //Create DB Context
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var optionBuilder = new DbContextOptionsBuilder<WHMapperContext>();
        optionBuilder.UseNpgsql(configuration["ConnectionStrings:DefaultConnection"]);

         _context = new WHMapperContext(optionBuilder.Options);
    }



    [Fact, TestPriority(100)]
    public async Task DeleteAndCreateDatabse()
    {
        //Delete all to make a fresh Db
        bool dbDeleted = await _context.Database.EnsureDeletedAsync();
        Assert.True(dbDeleted);
        bool dbCreated = await _context.Database.EnsureCreatedAsync();
        Assert.True(dbCreated);

    }

    [Fact, TestPriority(99)]
    public async Task CRUD_WHMAP()
    {
        //Create IWHMapRepository
        IWHMapRepository repo = new WHMapRepository(_context);

        //ADD WHMAP
        var result = await repo.Create(new WHMap(FOOBAR));
        Assert.NotNull(result);
        Assert.Equal(FOOBAR, result?.Name);

        //GetALL
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(FOOBAR, results?[0].Name);

        //GetByName
        var result2 = await repo.GetById(1);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR, result2.Name);

        //update
        result2.Name = FOOBAR_UPDATED;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_UPDATED, result2.Name);

        //Delete WHMAP
        var result5 = await repo.DeleteById(result2.Id);
        Assert.True(result5);
    }

    [Fact, TestPriority(98)]
    public async Task CRUD_WHSystem()
    {
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(_context);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);

        //Create IWHMapRepository
        IWHSystemRepository repo = new WHSystemRepository(_context);


        //GETALL system => return empty arry
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Empty(results);

        //ADD WHSystem
        var result = await repo.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(result);
        Assert.Equal(FOOBAR, result?.Name);
        Assert.Equal(1, result?.SecurityStatus);
        Assert.False(result?.Locked);

        //ADD Same WHsystem => return error null
        var duplicateResult = await repo.Create(new WHSystem(map.Id,FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.Null(duplicateResult);

        //GetALL
        results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(FOOBAR, results?[0].Name);
        Assert.Equal(1, results?[0].SecurityStatus);
        Assert.False(results?[0]?.Locked);

        //GetById
        Assert.NotNull(result);
        var result2 = await repo.GetById(result.Id);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_SYSTEM_ID, result2.SoloarSystemId);
        Assert.Equal(FOOBAR, result2.Name);
        Assert.Equal(1, result2.SecurityStatus);

        //update
        result2.Name = FOOBAR_UPDATED;
        result2.SoloarSystemId = FOOBAR_SYSTEM_ID2;
        result2.SecurityStatus = 0.5F;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_SYSTEM_ID2, result2?.SoloarSystemId);
        Assert.Equal(FOOBAR_UPDATED, result2?.Name);
        Assert.Equal(0.5F, result2?.SecurityStatus);


        //GetByName
        var result3 = await repo.GetByName(FOOBAR_UPDATED);
        Assert.NotNull(result3);
        Assert.Equal(FOOBAR_UPDATED, result3?.Name);
        Assert.Equal(0.5F, result3?.SecurityStatus);

        //Delete WHSytem
        var whDeleted = await repo.DeleteById(result.Id);
        Assert.True(whDeleted);

        //Delete WHMAP
        var mapDeleted= await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);
    }

    [Fact, TestPriority(97)]
    public async Task CRUD_WHLink()
    {
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(_context);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);


        //Create IWHMapRepository
        IWHSystemRepository repoWH = new WHSystemRepository(_context);
        Assert.NotNull(map);
        var whSys1 = await repoWH.Create(new WHSystem(map.Id,FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(whSys1);
        Assert.Equal(FOOBAR_SYSTEM_ID, whSys1.SoloarSystemId);
        Assert.Equal(FOOBAR, whSys1.Name);
        Assert.Equal(1, whSys1.SecurityStatus);
        Assert.False(whSys1.Locked);

        var whSys2 = await repoWH.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID2, FOOBAR_SHORT_UPDATED, 'A', 1));
        Assert.NotNull(whSys2);
        Assert.Equal(FOOBAR_SYSTEM_ID2, whSys2.SoloarSystemId);
        Assert.Equal(FOOBAR_SHORT_UPDATED, whSys2.Name);
        Assert.Equal(1, whSys2.SecurityStatus);
        Assert.Equal(Convert.ToByte('A'), whSys2.NameExtension);
        Assert.False(whSys2.Locked);


        //Create IWHMapRepository
        IWHSystemLinkRepository repo = new WHSystemLinkRepository(_context);

        //add whsystem link
        var link = await repo.Create(new WHSystemLink(map.Id,whSys1.Id, whSys2.Id));
        Assert.NotNull(link);
        Assert.Equal(whSys1.Id, link.IdWHSystemFrom);
        Assert.Equal(whSys2.Id, link.IdWHSystemTo);
        Assert.False(link.IsEndOfLifeConnection);
        Assert.Equal(SystemLinkMassStatus.Normal, link.MassStatus);
        Assert.Equal(SystemLinkSize.Large, link.Size);


        var linkGet = await repo.GetById(link.Id);
        Assert.NotNull(linkGet);
        Assert.Equal(link.Id, linkGet.Id);


        //update link
        link.IsEndOfLifeConnection = true;
        link.MassStatus = SystemLinkMassStatus.Verge;
        link = await repo.Update(link.Id, link);
        Assert.NotNull(link);
        Assert.Equal(whSys1.Id, link.IdWHSystemFrom);
        Assert.Equal(whSys2.Id, link.IdWHSystemTo);
        Assert.True(link.IsEndOfLifeConnection);
        Assert.Equal(SystemLinkMassStatus.Verge, link.MassStatus);
        Assert.Equal(SystemLinkSize.Large, link.Size);

        //Delete link
        var linkDel = await repo.DeleteById(link.Id);
        Assert.True(linkDel);

        //Delete WHMAP
        var mapDeleted = await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);

        var links = repo.GetAll();
        Assert.NotNull(links);

    }



    [Fact, TestPriority(96)]
    public async Task CRUD_WHSignature()
    {
        //init MAP
        //Create IWHMapRepository
        IWHMapRepository repoMap = new WHMapRepository(_context);

        //ADD WHMAP
        var map = await repoMap.Create(new WHMap(FOOBAR));
        Assert.NotNull(map);
        Assert.Equal(FOOBAR, map?.Name);


        //Create IWHMapRepository
        IWHSystemRepository repoWH = new WHSystemRepository(_context);
        Assert.NotNull(map);
        var whSys1 = await repoWH.Create(new WHSystem(map.Id, FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(whSys1);
        Assert.Equal(FOOBAR_SYSTEM_ID, whSys1.SoloarSystemId);
        Assert.Equal(FOOBAR, whSys1.Name);
        Assert.Equal(1, whSys1.SecurityStatus);
        Assert.False(whSys1.Locked);


        //Create IWHMapRepository
        IWHSignatureRepository repo = new WHSignatureRepository(_context);

        //ADD WHSignature
        var result = await repo.Create(new WHSignature(whSys1.Id,FOOBAR));
        Assert.NotNull(result);
        Assert.Equal(whSys1.Id, result.WHId);
        Assert.Equal(FOOBAR, result?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result?.Group);

        //GetALL
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(FOOBAR, results?[0].Name);
        Assert.Equal(WHSignatureGroup.Unknow, results?[0].Group);

        //GetById
        Assert.NotNull(result);
        var result2 = await repo.GetById(result.Id);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR, result2?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result2?.Group);

        //GetByWHId
        var result3 = await repo.GetByWHId(whSys1.Id);
        Assert.NotNull(result3);
        Assert.NotEmpty(result3);


        //GetByName
        result2 = await repo.GetByName(FOOBAR);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR, result2.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result2.Group);

        //update, name characters max 7
        result2.Name = FOOBAR_SHORT_UPDATED;
        result2.Group = WHSignatureGroup.Wormhole;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_SHORT_UPDATED, result2.Name);
        Assert.Equal(WHSignatureGroup.Wormhole, result2.Group);

        //updates ienumerable
        Assert.NotNull(results);
        results[0].UpdatedBy = FOOBAR;
        var resultsUpdates = await repo.Update(results);
        Assert.NotNull(resultsUpdates);
        Assert.Contains(resultsUpdates, x => x.UpdatedBy == FOOBAR);

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

        //delete all
        deleteStatus = await repo.DeleteByWHId(whSys1.Id);//test good system but empty sig
        Assert.True(deleteStatus);
       

        //Delete WHMAP, name characters max 7
        var result5 = await repo.DeleteById(result2.Id);
        Assert.True(result5);

        //Delete WHMAP
        var mapDeleted = await repoMap.DeleteById(map.Id);
        Assert.True(mapDeleted);

    }
}
