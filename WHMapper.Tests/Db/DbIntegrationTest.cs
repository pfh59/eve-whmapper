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
        Assert.Equal(FOOBAR, result2?.Name);

        //update
        result2.Name = FOOBAR_UPDATED;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_UPDATED, result2?.Name);



        //Add WHSystem
        var whSys1 = await repo.AddWHSystem(result2.Id, new WHSystem(FOOBAR_SYSTEM_ID, FOOBAR, 1));
        Assert.NotNull(whSys1);
        Assert.Equal(FOOBAR_SYSTEM_ID, whSys1?.SoloarSystemId);
        Assert.Equal(FOOBAR, whSys1?.Name);
        Assert.Equal(1, whSys1?.SecurityStatus);

        var whSys2 = await repo.AddWHSystem(result2.Id, new WHSystem(FOOBAR_SYSTEM_ID2,FOOBAR_SHORT_UPDATED,'A', 1));
        Assert.NotNull(whSys2);
        Assert.Equal(FOOBAR_SYSTEM_ID2, whSys2?.SoloarSystemId);
        Assert.Equal(FOOBAR_SHORT_UPDATED, whSys2?.Name);
        Assert.Equal(1, whSys2?.SecurityStatus);
        Assert.Equal(Convert.ToByte('A'), whSys2.NameExtension);

        //add whsystem link
        var link = await repo.AddWHSystemLink(result2.Id, whSys1.Id, whSys2.Id);
        Assert.NotNull(link);
        Assert.Equal(whSys1.Id, link.IdWHSystemFrom);
        Assert.Equal(whSys2.Id, link.IdWHSystemTo);
        Assert.False(link.IsEndOfLifeConnection);
        Assert.Equal(SystemLinkMassStatus.Normal, link.MassStatus);
        Assert.Equal(SystemLinkSize.Large, link.Size);

        //Delete link
        var linkDel = await repo.RemoveWHSystemLink(result2.Id, link.Id);
        Assert.NotNull(linkDel);
        Assert.Equal(link.Id, linkDel.IdWHSystemFrom);

        //Delete byname
        var whSys2Del = await repo.RemoveWHSystemByName(result2.Id, FOOBAR_SHORT_UPDATED);
        Assert.NotNull(whSys2Del);
        Assert.Equal(FOOBAR_SHORT_UPDATED, whSys2Del?.Name);
        Assert.Equal(1, whSys2Del?.SecurityStatus);

        //Delete byname
        var whSys1Del = await repo.RemoveWHSystem(result2.Id, whSys1.Id);
        Assert.NotNull(whSys1Del);
        Assert.Equal(FOOBAR, whSys1Del?.Name);
        Assert.Equal(1, whSys1Del?.SecurityStatus);


        //Delete WHMAP
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal(FOOBAR_UPDATED, result5?.Name);
    }

    [Fact, TestPriority(98)]
    public async Task CRUD_WHSystem()
    {
        //Create IWHMapRepository
        IWHSystemRepository repo = new WHSystemRepository(_context);

        //GETALL system => return empty arry
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Empty(results);

        //ADD WHSystem
        var result = await repo.Create(new WHSystem(FOOBAR_SYSTEM_ID,FOOBAR, 1));
        Assert.NotNull(result);
        Assert.Equal(FOOBAR, result?.Name);
        Assert.Equal(1, result?.SecurityStatus);

        //ADD Same WHsystem => return error null
        var duplicateResult = await repo.Create(new WHSystem(FOOBAR_SYSTEM_ID,FOOBAR, 1));
        Assert.Null(duplicateResult);

        //GetALL
        results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(FOOBAR, results?[0].Name);
        Assert.Equal(1, results?[0].SecurityStatus);

        //GetById
        var result2 = await repo.GetById(result.Id);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_SYSTEM_ID, result2?.SoloarSystemId);
        Assert.Equal(FOOBAR, result2?.Name);
        Assert.Equal(1, result2?.SecurityStatus);

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



        //remove ALL WHSystemSignature
       var badSystemIdResult = await repo.RemoveAllWHSignature(123);//test bad system id return false
       Assert.False(badSystemIdResult);


       var goodSystemIdResult=await repo.RemoveAllWHSignature(result2.Id);//test good system but empty sig
       Assert.True(goodSystemIdResult);


        //Add WHSystemSignature
        var resultWHSignature = await repo.AddWHSignature(result2.Id, new WHSignature(FOOBAR));
        Assert.NotNull(resultWHSignature);
        Assert.Equal(FOOBAR, resultWHSignature?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, resultWHSignature?.Group);

        //Remove WHSystemSignature
        var resultWHSignatureDel = await repo.RemoveWHSignature(result2.Id, resultWHSignature.Id);
        Assert.NotNull(resultWHSignatureDel);
        Assert.Equal(FOOBAR, resultWHSignatureDel?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, resultWHSignatureDel?.Group);

        //Add multi sig
        IList<WHSignature> sigs = new List<WHSignature>();
        sigs.Add(new WHSignature(FOOBAR));
        sigs.Add(new WHSignature(FOOBAR_SHORT_UPDATED));


        var resultWHSigs = await repo.AddWHSignatures(result2.Id, sigs);
        Assert.NotNull(resultWHSigs);
        Assert.Equal(2, resultWHSigs.Count());

        //delete all
        goodSystemIdResult = await repo.RemoveAllWHSignature(result2.Id);//test good system but empty sig
        Assert.True(goodSystemIdResult);

        //Delete WHSystem
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal(FOOBAR_UPDATED, result5?.Name);
        Assert.Equal(0.5F, result2?.SecurityStatus);
    }

    [Fact, TestPriority(97)]
    public async Task CRUD_WHSignature()
    {
        //Create IWHMapRepository
        IWHSignatureRepository repo = new WHSignatureRepository(_context);

        //ADD WHSignature
        var result = await repo.Create(new WHSignature(FOOBAR));
        Assert.NotNull(result);
        Assert.Equal(FOOBAR, result?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result?.Group);

        //GetALL
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(FOOBAR, results?[0].Name);
        Assert.Equal(WHSignatureGroup.Unknow, results?[0].Group);

        //GetById
        var result2 = await repo.GetById(result.Id);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR, result2?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result2?.Group);

        //GetByName
        result2 = await repo.GetByName(FOOBAR);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR, result2?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result2?.Group);

        //update, name characters max 7
        result2.Name = FOOBAR_SHORT_UPDATED;
        result2.Group = WHSignatureGroup.Wormhole;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_SHORT_UPDATED, result2?.Name);
        Assert.Equal(WHSignatureGroup.Wormhole, result2?.Group);

        //Delete WHMAP, name characters max 7
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal(FOOBAR_SHORT_UPDATED, result5?.Name);
        Assert.Equal(WHSignatureGroup.Wormhole, result5?.Group);
     
    }
}
