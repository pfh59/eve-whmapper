using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using WHMapper.Data;
using WHMapper.Services.Cache;
using WHMapper.Services.WHActivityLogs;

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

        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            logger.LogInformation("Migrating database...");
            try
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrated successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }
    }

    /// <summary>
    /// Deletes activities older than the configured retention period of the activity log.
    /// </summary>
    /// <remarks>
    /// A failure is logged and does not stop the application startup.
    /// </remarks>
    public static async Task PurgeExpiredActivityLogsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var activityLogService = scope.ServiceProvider.GetRequiredService<IWHActivityLogService>();
            if (activityLogService.RetentionDays <= 0)
            {
                logger.LogInformation("Activity log retention is disabled; no activity purged.");
                return;
            }

            int deletedCount = await activityLogService.PurgeExpiredAsync();
            logger.LogInformation("{DeletedCount} activities older than {RetentionDays} days purged.", deletedCount, activityLogService.RetentionDays);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to purge expired activities, but application will continue.");
        }
    }
}
