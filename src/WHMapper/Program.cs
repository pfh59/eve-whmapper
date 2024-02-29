using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using WHMapper.Data;
using WHMapper.Models.DTO;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHSystems;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveOAuthProvider;
using Microsoft.AspNetCore.Components.Authorization;
using WHMapper.Services.EveJwtAuthenticationStateProvider;
using WHMapper.Services.WHColor;
using WHMapper.Repositories.WHSystemLinks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using WHMapper.Hubs;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Services.EveJwkExtensions;
using System.Net;
using WHMapper.Services.EveOnlineUserInfosProvider;
using MudBlazor;
using WHMapper.Services.WHSignature;
using WHMapper.Services.WHSignatures;
using WHMapper.Services.SDE;
using WHMapper.Services.EveMapper;
using WHMapper.Repositories.WHAdmins;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Services.EveAPI.Character;
using WHMapper.Services.EveMapper.AuthorizationPolicies;
using WHMapper.Repositories.WHNotes;
using WHMapper.Services.Cache;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using WHMapper.Repositories.WHJumpLogs;


namespace WHMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContextFactory<WHMapperContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection")));
                    

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection"));

            builder.Services.AddDataProtection()
                .SetApplicationName("WHMapper")
                .PersistKeysToStackExchangeRedis(redis);


            builder.Services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
                option.InstanceName = "WHMapper";
            });

            builder.Services.AddSignalR();

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor(options =>
            {
                options.DetailedErrors = false;
                options.DisconnectedCircuitMaxRetained = 100;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(30);
                options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(15);
                options.MaxBufferedUnacknowledgedRenderBatches = 10;
            });

            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

                config.SnackbarConfiguration.PreventDuplicates = true;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 10000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });

            //signalR  compression
            builder.Services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });


            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });


            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookieValue = "true";
            });


            IConfigurationSection evessoConf = builder.Configuration.GetSection("EveSSO");
            IConfigurationSection evessoConfScopes = evessoConf.GetSection("DefaultScopes");


            AuthenticationBuilder authenticationBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(12);
                options.AccessDeniedPath = "/Forbidden/";
            })
            .AddEVEOnline(EVEOnlineAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = evessoConf["ClientId"];
                options.ClientSecret = evessoConf["Secret"];
                options.CallbackPath = new PathString("/sso/callback");
                options.Scope.Clear();

                foreach (string scope in evessoConfScopes.Get<string[]>())
                    options.Scope.Add(scope);


                options.SaveTokens = true;
                options.UsePkce = true;
            })
            .AddEveOnlineJwtBearer();//validate hub tokken



            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Access", policy =>
                    policy.Requirements.Add(new EveMapperAccessRequirement()));

                options.AddPolicy("Admin", policy =>
                    policy.Requirements.Add(new EveMapperAdminRequirement()));
            });


            
            
            using (var serviceScope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<WHMapperContext>();
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<WHMapperContext>();

                int attempt = 0;
                while (!dbContext.Database.CanConnect() && attempt < 10)
                int attempt = 0;
                while (!dbContext.Database.CanConnect() && attempt < 10)
                {
                    logger.LogWarning("Database not ready yet.Attempt {0}/10", attempt);
                    logger.LogWarning("Database not ready yet.Attempt {0}/10", attempt);
                    Thread.Sleep(1000);
                    attempt++;
                }

                if (attempt >= 10)
                {
                    logger.LogError("Database not ready after 10 attempts; exiting.");
                    return ;
                }


                if(dbContext.Database.GetPendingMigrations().Any())
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
                    attempt++;
                }

                if (attempt >= 10)
                {
                    logger.LogError("Database not ready after 10 attempts; exiting.");
                    return ;
                }


                if(dbContext.Database.GetPendingMigrations().Any())
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


            builder.Services.AddSingleton<IConfiguration>(provider => builder.Configuration);
            builder.Services.AddHttpClient();

            builder.Services.AddScoped<TokenProvider>();
            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services.AddScoped<AuthenticationStateProvider, EveAuthenticationStateProvider>();
            


            builder.Services.AddScoped<IEveUserInfosServices, EveUserInfosServices>();
            builder.Services.AddScoped<IEveAPIServices, EveAPIServices>();
            builder.Services.AddScoped<ICharacterServices, CharacterServices>();
            builder.Services.AddScoped<IAnoikServices, AnoikServices>();
            builder.Services.AddScoped<ISDEServices, SDEServices>();
            

            #region DB Acess Repo
            builder.Services.AddScoped<IWHAdminRepository, WHAdminRepository>();
            builder.Services.AddScoped<IWHAccessRepository, WHAccessRepository>();
            builder.Services.AddScoped<IWHMapRepository, WHMapRepository>();
            builder.Services.AddScoped<IWHSystemRepository, WHSystemRepository>();
            builder.Services.AddScoped<IWHSignatureRepository, WHSignatureRepository>();
            builder.Services.AddScoped<IWHSystemLinkRepository, WHSystemLinkRepository>();
            builder.Services.AddScoped<IWHNoteRepository, WHNoteRepository>();
            builder.Services.AddScoped<IWHRouteRepository, WHRouteRepository>();
            builder.Services.AddScoped<IWHJumpLogRepository,WHJumpLogRepository>();

            builder.Services.AddScoped<IWHJumpLogRepository,WHJumpLogRepository>();

            #endregion

            #region WH HELPER
            builder.Services.AddScoped<IEveMapperAccessHelper, EveMapperAccessHelper>();
            builder.Services.AddScoped<IEveMapperTracker, EveMapperTracker>();
            builder.Services.AddScoped<IEveMapperSearch, EveMapperSearch>();
            builder.Services.AddScoped<IEveMapperHelper, EveMapperHelper>();
            builder.Services.AddScoped<IEveMapperRoutePlannerHelper, EveMapperRoutePlannerHelper>();
            builder.Services.AddScoped<IWHSignatureHelper, WHSignatureHelper>();
            builder.Services.AddScoped<IWHColorHelper, WHColorHelper>();
            #endregion

            builder.Services.AddScoped<IPasteServices,PasteServices>();

            builder.Services.AddScoped<IAuthorizationHandler, EveMapperAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, EveMapperAdminHandler>();



            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                });
            }

            var app = builder.Build();

            if (app.Environment.IsProduction())
            {
                app.Use((context, next) =>
                {
                    context.Request.Scheme = "https";
                    return next(context);
                });
            }
            app.UseForwardedHeaders();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapBlazorHub();
            app.MapHub<WHMapperNotificationHub>("/whmappernotificationhub");//signalR
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }

}
