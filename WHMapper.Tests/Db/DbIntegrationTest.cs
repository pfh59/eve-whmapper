using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WHMapper.Data;

namespace WHMapper.Tests;

[Collection("Database")]
public class DbIntegrationTest
{
    [Fact]
    public async Task DeleteAndCreateDatabse()
    {
            //Create DB Context
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var optionBuilder = new DbContextOptionsBuilder<WHMapperContext>();
            optionBuilder.UseNpgsql(configuration["DefaultConnection"]);

            var context = new WHMapperContext(optionBuilder.Options);

            //Delete all to make a fresh Db
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
    }
}
