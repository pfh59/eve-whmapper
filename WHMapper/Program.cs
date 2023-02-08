using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using WHMapper.Data;
using WHMapper.Models.DTO;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHSystems;
using WHMapper.Repositories.WHSignatures;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveOAuthProvider;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WHMapper.Services.EveJwtAuthenticationStateProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Extensions.Options;
using WHMapper.Services.WHColor;
using WHMapper.Repositories.WHSystemLinks;
using System;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<WHMapperContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);


builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();


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
});


IConfigurationSection evessoConf = builder.Configuration.GetSection("EveSSO");
AuthenticationBuilder authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = EVEOnlineAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddEVEOnline(EVEOnlineAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ClientId = evessoConf["ClientId"];
    options.ClientSecret = evessoConf["Secret"];
    options.CallbackPath = new PathString("/sso/callback");
    options.Scope.Clear();
    options.Scope.Add("esi-location.read_location.v1");
    options.Scope.Add("esi-location.read_ship_type.v1");
    options.Scope.Add("esi-ui.open_window.v1");
    options.Scope.Add("esi-ui.write_waypoint.v1");
    options.SaveTokens = true;
    options.UsePkce = true;
});




using (var serviceScope = builder.Services.BuildServiceProvider().CreateScope())
{
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = serviceScope.ServiceProvider.GetRequiredService<WHMapperContext>().Database;

    logger.LogInformation("Migrating database...");

    while (!db.CanConnect())
    {
        logger.LogInformation("Database not ready yet; waiting...");
        Thread.Sleep(1000);
    }

    try
    {
        serviceScope.ServiceProvider.GetRequiredService<WHMapperContext>().Database.Migrate();
        logger.LogInformation("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}


builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConfiguration>(provider => builder.Configuration);

builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, EveJWTAuthenticationStateProvider>();

builder.Services.AddScoped<IEveAPIServices, EveAPIServices>();

builder.Services.AddSingleton<IAnoikServices, AnoikServices>();

builder.Services.AddScoped<IWHMapRepository, WHMapRepository>();
builder.Services.AddScoped<IWHSystemRepository, WHSystemRepository>();
builder.Services.AddScoped<IWHSignatureRepository, WHSignatureRepository>();
builder.Services.AddScoped<IWHSystemLinkRepository, WHSystemLinkRepository>();

builder.Services.AddSingleton<IWHColorHelper, WHColorHelper>();

var app = builder.Build();

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
//app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

