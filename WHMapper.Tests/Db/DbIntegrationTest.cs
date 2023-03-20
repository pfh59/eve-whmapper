using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystems;
using WHMapper.Tests.Attributes;
using static MudBlazor.CategoryTypes;

namespace WHMapper.Tests.Db;

[TestCaseOrderer("WHMapper.Tests.Orderers.PriorityOrderer", "WHMapper.Tests.Db")]
public class DbIntegrationTest
{

    private const string FOOBAR ="FooBar";
    private const string FOOBAR_UPDATED = "FooBar Updated";
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

        //Delete WHMAP
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal(FOOBAR_UPDATED, result5?.Name);
    }

    [Fact, TestPriority(99)]
    public async Task CRUD_WHSystem()
    {
        //Create IWHMapRepository
        IWHSystemRepository repo = new WHSystemRepository(_context);

        //ADD WHMAP
        var result = await repo.Create(new WHSystem(FOOBAR, 1));
        Assert.NotNull(result);
        Assert.Equal(FOOBAR, result?.Name);
        Assert.Equal(1, result?.SecurityStatus);

        //GetALL
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(FOOBAR, results?[0].Name);
        Assert.Equal(1, results?[0].SecurityStatus);

        //GetByName
        var result2 = await repo.GetById(1);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR, result2?.Name);
        Assert.Equal(1, result2?.SecurityStatus);

        //update
        result2.Name = FOOBAR_UPDATED;
        result2.SecurityStatus = 0.5F;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal(FOOBAR_UPDATED, result2?.Name);
        Assert.Equal(0.5F, result2?.SecurityStatus);

        //Delete WHMAP
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal(FOOBAR_UPDATED, result5?.Name);
        Assert.Equal(0.5F, result2?.SecurityStatus);

    }

    [Fact, TestPriority(98)]
    public async Task CRUD_WHSignature()
    {
        //Create IWHMapRepository
        IWHSignatureRepository repo = new WHSignatureRepository(_context);

        //ADD WHMAP
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

        //GetByName
        var result2 = await repo.GetById(1);
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
