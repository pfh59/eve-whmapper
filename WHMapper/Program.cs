using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    //options.DefaultAuthenticateScheme = "bearer";


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
/*
.AddJwtBearer("bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the server key used to sign the JWT token is here, use more than 16 chars")),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero //the default for this setting is 5 minutes
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});*/

IConfigurationSection? eveapiConf = builder.Configuration.GetSection("EveAPI");
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
builder.Services.AddScoped<IWHSignature, WHSystemRepository>();
builder.Services.AddScoped<IWHSignatureRepository, WHSignatureRepository>();

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

