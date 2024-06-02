using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Services.Cache;

namespace WHMapper.Tests.Services;

[Collection("C1-Services")]
public class CacheIntegrationTests
{
    private readonly ICacheService? _services;
    private readonly ICacheService? _badServices;
    public CacheIntegrationTests()
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
        if(provider != null)
        {
            IDistributedCache? _distriCache = provider.GetService<IDistributedCache>();
            ILogger<CacheService> loggerCache = new NullLogger<CacheService>();

            if(_distriCache != null && loggerCache != null)
            {
                _services = new CacheService(loggerCache,_distriCache);
                Assert.NotNull(_services);
                
                _badServices = new CacheService(loggerCache,null!);
                Assert.NotNull(_badServices);
            }
        }
    }

    [Fact]
    public async Task Set_Get_Remove()
    {
        var key = "test";
        var value = "test";

        var key2 = "test2";
        var value2 = "test2";

        Assert.NotNull(_services);

        bool successSet = await _services.Set(key,value);
        Assert.True(successSet);

        var result = await _services.Get<string>(key);
        Assert.NotNull(result);
        Assert.Equal(value,result);

        bool successSetTimed = await _services.Set(key2,value2,TimeSpan.FromSeconds(5));
        Assert.True(successSetTimed);

        await Task.Delay(TimeSpan.FromSeconds(7));

        var result2 = await _services.Get<string>(key2);
        Assert.Null(result2);

        bool successDel = await _services.Remove(key);
        Assert.True(successDel);
    }

    [Fact]
    public async Task Set_Get_Remove_With_Bad_Config()
    {
        

        var key = "test";
        var value = "test";

        var key2 = "test2";
        var value2 = "test2";

        Assert.NotNull(_badServices);
        bool badSet = await _badServices.Set(key,value);
        Assert.False(badSet);

        var result = await _badServices.Get<string>(key);
        Assert.Null(result);

        bool badSetTimed = await _badServices.Set(key2,value2,TimeSpan.FromSeconds(5));
        Assert.False(badSetTimed);

        var result2 = await _badServices.Get<string>(key2);
        Assert.Null(result2);

        bool badDel = await _badServices.Remove(key);
        Assert.False(badDel);
    }

}