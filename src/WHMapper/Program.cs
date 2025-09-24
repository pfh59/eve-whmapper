using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using WHMapper.Services.EveOAuthProvider;
using WHMapper.Services.EveJwkExtensions;
using Microsoft.AspNetCore.ResponseCompression;
using System.Net;
using WHMapper.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using WHMapper.Services.EveMapper.AuthorizationPolicies;
using WHMapper.Services.Cache;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.Anoik;
using WHMapper.Services.SDE;
using WHMapper.Repositories.WHAdmins;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHSystems;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Repositories.WHSystemLinks;
using WHMapper.Repositories.WHNotes;
using WHMapper;
using WHMapper.Repositories.WHJumpLogs;
using WHMapper.Models.DTO;
using WHMapper.Services.EveMapper;
using WHMapper.Services.WHSignature;
using WHMapper.Services.WHColor;
using Microsoft.AspNetCore.Authorization;
using WHMapper.Services.WHSignatures;
using WHMapper.Hubs;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using WHMapper.Services.BrowserClientIdProvider;
using WHMapper.Services.EveCookieExtensions;
using Microsoft.AspNetCore.HttpOverrides;
using WHMapper.Components;
using Serilog;
using WHMapper.Services.BrowserClientIdProvider.Extension;
using WHMapper.Services.EveScoutAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});


builder.Services.AddDbContextFactory<WHMapperContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection")));


var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection") 
    ?? throw new InvalidOperationException("RedisConnection is not configured in the settings.");
ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);

builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    option.InstanceName = "WHMapper";
});

builder.Services.AddDataProtection()
    .SetApplicationName("WHMapper")
    .PersistKeysToStackExchangeRedis(redis,"DataProtection-Keys");


IConfigurationSection evessoConf = builder.Configuration.GetSection("EveSSO");
IConfigurationSection evessoConfScopes = evessoConf.GetSection("DefaultScopes");

builder.Services.AddAuthentication(EVEOnlineAuthenticationDefaults.AuthenticationScheme)
.AddEVEOnline(EVEOnlineAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ClientId = evessoConf["ClientId"] ?? throw new InvalidOperationException("ClientId is not configured in EveSSO settings.");
    options.ClientSecret = evessoConf["Secret"] ?? throw new InvalidOperationException("Secret is not configured in EveSSO settings.");
    options.CallbackPath = new PathString("/sso/callback");
    options.Scope.Clear();

    var scopes = evessoConfScopes.Get<string[]>();
    if (scopes != null)
    {
        foreach (string scope in scopes)
            options.Scope.Add(scope);
    }
    
    options.SaveTokens = true;
    options.UsePkce = true;

    options.Events = new OAuthEvents
    {
        OnTicketReceived = async context =>
        {
            var userManagerService = context.HttpContext.RequestServices.GetRequiredService<IEveMapperUserManagementService>();
           
            var clientId = context.Properties?.GetTokenValue("clientId");
            
            if (string.IsNullOrEmpty(clientId))
            {
                await Task.FromException(new Exception("ClientId is not set."));
                return;
            }

            var accountId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountId))
            {
                await Task.FromException(new Exception("AccountId is not set."));
                return;
            }
            var accessToken = context.Properties?.GetTokenValue("access_token");
            var refreshToken = context.Properties?.GetTokenValue("refresh_token");
            var expiresAt = context.Properties?.GetTokenValue("expires_at");

            
            if (!DateTimeOffset.TryParse(expiresAt, out var accessTokenExpiration))
            {
                await Task.FromException(new Exception("Invalid access token expiration date."));
                return;
            }

            var token = new UserToken
            {
                AccountId = accountId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiry = accessTokenExpiration.UtcDateTime
            };

            await userManagerService.AddAuthenticateWHMapperUser(clientId,accountId, token);

            await Task.CompletedTask;
        }

    };

})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "WHMapper";
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Forbidden/";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddEveOnlineJwtBearer();//validate hub tokken
builder.Services.ConfigureEveCookieRefresh(CookieAuthenticationDefaults.AuthenticationScheme, EVEOnlineAuthenticationDefaults.AuthenticationScheme);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Trust All Proxies
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.ConsentCookieValue = "true";
});

builder.Services.AddSignalR();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Access", policy =>
        policy.Requirements.Add(new EveMapperAccessRequirement()));

    options.AddPolicy("Admin", policy =>
        policy.Requirements.Add(new EveMapperAdminRequirement()));

    options.AddPolicy("Map", policy =>
        policy.Requirements.Add(new EveMapperMapRequirement()));
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;

    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddSingleton<IConfiguration>(provider => builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IEveAPIServices, EveAPIServices>(client =>
{
    client.BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl);
});//.AddHttpMessageHandler<EveOnlineAccessTokenHandler>();

builder.Services.AddHttpClient<ICharacterServices, CharacterServices>(client =>
{
    client.BaseAddress = new Uri(EveAPIServiceConstants.ESIUrl);
});//.AddHttpMessageHandler<EveOnlineAccessTokenHandler>();

builder.Services.AddHttpClient<IEveScoutAPIServices, EveScoutAPIServices>(client =>
{
    client.BaseAddress = new Uri(EveScoutAPIServiceConstants.EveScoutUrl);
});

builder.Services.AddScoped<IAnoikDataSupplier>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var jsonFilePath = configuration["AnoikDataSupplier:JsonFilePath"];
    if (string.IsNullOrEmpty(jsonFilePath))
    {
        throw new InvalidOperationException("JsonFilePath is not configured in AnoikDataSupplier settings.");
    }
    return new AnoikJsonDataSupplier(jsonFilePath);
});


builder.Services.AddScoped<IAnoikServices, AnoikServices>();
builder.Services.AddScoped<ISDEService, SDEService>();
builder.Services.AddScoped<ISDEServiceManager, SDEServiceManager>();
builder.Services.AddScoped<ISDEDataSupplier, SdeDataSupplier>();
builder.Services.AddHttpClient<ISDEDataSupplier, SdeDataSupplier>(client =>
{
    var baseUrl = builder.Configuration["SdeDataSupplier:BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is not configured in SdeDataSupplier settings.");
    client.BaseAddress = new Uri(baseUrl);
}); 


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
#endregion

#region WH HELPER
builder.Services.AddSingleton<IBrowserClientIdProvider, BrowserClientIdProvider>();
builder.Services.AddScoped<ClientUID>();
builder.Services.AddSingleton<IEveMapperUserManagementService,EveMapperUserManagementService>();

builder.Services.AddScoped<IEveMapperService, EveMapperService>();
builder.Services.AddScoped<IEveMapperCacheService, EveMapperCacheService>();
builder.Services.AddScoped<IEveMapperAccessHelper, EveMapperAccessHelper>();
builder.Services.AddScoped<IEveMapperTracker, EveMapperTracker>();
builder.Services.AddScoped<IEveMapperSearch, EveMapperSearch>();
builder.Services.AddScoped<IEveMapperHelper, EveMapperHelper>();
builder.Services.AddScoped<IEveMapperRoutePlannerHelper, EveMapperRoutePlannerHelper>();
builder.Services.AddScoped<IWHSignatureHelper, WHSignatureHelper>();
builder.Services.AddScoped<IWHColorHelper, WHColorHelper>();
builder.Services.AddScoped<IEveMapperRealTimeService,EveMapperRealTimeService>();
#endregion

builder.Services.AddScoped<IPasteServices,PasteServices>();

builder.Services.AddScoped<IAuthorizationHandler, EveMapperAccessHandler>();
builder.Services.AddScoped<IAuthorizationHandler, EveMapperAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, EveMapperMapHandler>();


//signalR  compression
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
    });
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<WHMapperContext>();

    int attempt = 0;
    while (!dbContext.Database.CanConnect() && attempt < 10)
    {
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
    }
}



if (app.Environment.IsProduction())
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next(context);
    });
}
app.UseForwardedHeaders();
app.UseResponseCompression();
app.UseMiddleware<BrowserClientIdCookieMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


//check if applicationUrls contains https
var applicationUrls = builder.Configuration["Kestrel:Endpoints:Https:Url"];
if ((!string.IsNullOrEmpty(applicationUrls) && app.Environment.IsProduction()) || app.Environment.IsDevelopment())
{
    Log.Information("Using HTTPS redirection");
    app.UseHttpsRedirection();
}
else
{
    Log.Warning("HTTPS redirection is not enabled. Please ensure that the application is behind a reverse proxy that handles HTTPS.");
}

app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<WHMapperNotificationHub>("/whmappernotificationhub");
app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();
