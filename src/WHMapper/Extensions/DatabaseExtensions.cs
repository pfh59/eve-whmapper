using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using WHMapper.Data;
using WHMapper.Services.Cache;

namespace WHMapper.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<WHMapperContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        var redisConnectionString = configuration.GetConnectionString("RedisConnection")
            ?? throw new InvalidOperationException("RedisConnection is not configured in the settings.");
        var redis = ConnectionMultiplexer.Connect(redisConnectionString);

        services.AddStackExchangeRedisCache(option =>
        {
            option.Configuration = configuration.GetConnectionString("RedisConnection");
            option.InstanceName = "WHMapper";
        });

        services.AddDataProtection()
            .SetApplicationName("WHMapper")
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");

        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WHMapperContext>();

        int attempt = 0;
        while (!await dbContext.Database.CanConnectAsync() && attempt < 10)
        {
            attempt++;
            logger.LogWarning("Database not ready yet. Attempt {Attempt}/10", attempt);
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)));
        }

        if (attempt >= 10)
        {
            logger.LogError("Database not ready after 10 attempts; exiting.");
            Environment.Exit(1);
        }

        if (dbContext.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Migrating database...");
            try
            {
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrated successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }
    }
}
