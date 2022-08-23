using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystems;
using WHMapper.Tests.Attributes;

namespace WHMapper.Tests.Db;

[TestCaseOrderer("WHMapper.Tests.Orderers.PriorityOrderer", "WHMapper.Tests.Db")]
public class DbIntegrationTest
{
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
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();
    }

    [Fact, TestPriority(99)]
    public async Task CRUD_WHMAP()
    {
        //Create IWHMapRepository
        IWHMapRepository repo = new WHMapRepository(_context);

        //ADD WHMAP
        var result = await repo.Create(new WHMap("FooBar"));
        Assert.NotNull(result);
        Assert.Equal("FooBar", result?.Name);

        //GetALL
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("FooBar", results?[0].Name);

        //GetByName
        var result2 = await repo.GetById(1);
        Assert.NotNull(result2);
        Assert.Equal("FooBar", result2?.Name);

        //update
        result2.Name = "FooBar Updated";
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal("FooBar Updated", result2?.Name);

        //Delete WHMAP
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal("FooBar Updated", result5?.Name);
    }

    [Fact, TestPriority(99)]
    public async Task CRUD_WHSystem()
    {
        //Create IWHMapRepository
        IWHSignature repo = new WHSystemRepository(_context);

        //ADD WHMAP
        var result = await repo.Create(new WHSystem("FooBar",1));
        Assert.NotNull(result);
        Assert.Equal("FooBar", result?.Name);
        Assert.Equal(1, result?.SecurityStatus);

        //GetALL
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("FooBar", results?[0].Name);
        Assert.Equal(1, results?[0].SecurityStatus);

        //GetByName
        var result2 = await repo.GetById(1);
        Assert.NotNull(result2);
        Assert.Equal("FooBar", result2?.Name);
        Assert.Equal(1, result2?.SecurityStatus);

        //update
        result2.Name = "FooBar Updated";
        result2.SecurityStatus = 0.5F;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal("FooBar Updated", result2?.Name);
        Assert.Equal(0.5F, result2?.SecurityStatus);

        //Delete WHMAP
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal("FooBar Updated", result5?.Name);
        Assert.Equal(0.5F, result2?.SecurityStatus);
    }

    [Fact, TestPriority(98)]
    public async Task CRUD_WHSignature()
    {
        //Create IWHMapRepository
        IWHSignatureRepository repo = new WHSignatureRepository(_context);

        //ADD WHMAP
        var result = await repo.Create(new WHSignature("FooBar"));
        Assert.NotNull(result);
        Assert.Equal("FooBar", result?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result?.Group);

        //GetALL
        var results = (await repo.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("FooBar", results?[0].Name);
        Assert.Equal(WHSignatureGroup.Unknow, results?[0].Group);

        //GetByName
        var result2 = await repo.GetById(1);
        Assert.NotNull(result2);
        Assert.Equal("FooBar", result2?.Name);
        Assert.Equal(WHSignatureGroup.Unknow, result2?.Group);

        //update, name characters max 7
        result2.Name = "FooBarU";
        result2.Group = WHSignatureGroup.Wormhole;
        var result4 = await repo.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal("FooBarU", result2?.Name);
        Assert.Equal(WHSignatureGroup.Wormhole, result2?.Group);

        //Delete WHMAP, name characters max 7
        var result5 = await repo.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal("FooBarU", result5?.Name);
        Assert.Equal(WHSignatureGroup.Wormhole, result5?.Group);
    }
}
