using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WHMapper.Data;
using WHMapper.Models.Db;
using WHMapper.Repositories.WHMaps;
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
        IWHMapRepository repoWHMAP = new WHMapRepository(_context);

        //ADD WHMAP
        var result = await repoWHMAP.Create(new WHMap("FooBar"));
        Assert.NotNull(result);
        Assert.Equal("FooBar", result?.Name);

        //GetALL
        var results = (await repoWHMAP.GetAll())?.ToArray();
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("FooBar", results?[0].Name);

        //GetByName
        var result2 = await repoWHMAP.GetById(1);
        Assert.NotNull(result2);
        Assert.Equal("FooBar", result2?.Name);

        //update
        result2.Name = "FooBar Updated";
        var result4 = await repoWHMAP.Update(result2.Id, result2);
        Assert.NotNull(result2);
        Assert.Equal("FooBar Updated", result2?.Name);

        //Delete WHMAP
        var result5 = await repoWHMAP.DeleteById(result2.Id);
        Assert.NotNull(result5);
        Assert.Equal("FooBar Updated", result5?.Name);
    }
}
