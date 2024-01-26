using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WHMapper.Services.Cache;

namespace WHMapper.Tests.Services;

[Collection("C1-Services")]
public class CacheTest
{
    private readonly ICacheService _services=null!;

    public CacheTest()
    {
        var services = new ServiceCollection();


        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

  
        var provider = services.BuildServiceProvider();
        if(provider != null)
        {
            IDistributedCache? _distriCache = provider.GetService<IDistributedCache>();
            ILogger<CacheService> loggerCache = new NullLogger<CacheService>();

            if(_distriCache != null && loggerCache != null)
                _services = new CacheService(loggerCache,_distriCache);
        }
    }

    [Fact]
    public async Task Set_Get_Remove()
    {
        var key = "test";
        var value = "test";

        var key2 = "test2";
        var value2 = "test2";

        bool successSet = await _services.Set(key,value);
        Assert.True(successSet);

        var result = await _services.Get<string>(key);
        Assert.NotNull(result);
        Assert.Equal(value,result);

        bool successSetTimed = await _services.Set(key2,value2,TimeSpan.FromSeconds(5));
        Assert.True(successSetTimed);

        await Task.Delay(TimeSpan.FromSeconds(6));

        var result2 = await _services.Get<string>(key2);
        Assert.Null(result2);

        bool successDel = await _services.Remove(key);
        Assert.True(successDel);
    }
}