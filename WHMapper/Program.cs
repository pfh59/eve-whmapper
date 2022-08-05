using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using WHMapper.Data;
using WHMapper.Models.DTO;
using WHMapper.Repositories.WHMaps;
using WHMapper.Repositories.WHSystems;
using WHMapper.Services.Anoik;
using WHMapper.Services.EveAPI;
using WHMapper.Services.EveOAuthProvider;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<WHMapperContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();




builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Add authentication services
IConfigurationSection evessoConf = builder.Configuration.GetSection("EveSSO");
AuthenticationBuilder authenticationBuilder = builder.Services.AddAuthentication(options =>
{

    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = EVEOnlineAuthenticationDefaults.AuthenticationScheme;


})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddEVEOnline(EVEOnlineAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.ClientId = evessoConf["ClientId"];
    options.ClientSecret = evessoConf["Secret"];
    options.CallbackPath = new PathString("/callback");
    options.Scope.Clear();
    options.Scope.Add("esi-location.read_location.v1");
    options.Scope.Add("esi-location.read_ship_type.v1");
    options.Scope.Add("esi-ui.open_window.v1");
    options.Scope.Add("esi-ui.write_waypoint.v1");

    options.SaveTokens = true;
});


IConfigurationSection eveapiConf = builder.Configuration.GetSection("EveAPI");
builder.Services.AddHttpClient("EveAPI", client =>
{
    client.BaseAddress = new Uri($"https://{eveapiConf["Domain"]}");
});

builder.Services.AddScoped<TokenProvider>();

builder.Services.AddTransient<IEveAPIServices, EveAPIServices>(sp =>
{
    var tokenProvider = sp.GetRequiredService<TokenProvider>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("EveAPI");


    return new EveAPIServices(httpClient, tokenProvider);
});


builder.Services.AddScoped<IAnoikServices, AnoikServices>();

builder.Services.AddScoped<IWHMapRepository, WHMapRepository>();
builder.Services.AddScoped<IWHSystemRepository, WHSystemRepository>();

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

