using MudBlazor;
using MudBlazor.Services;
using Microsoft.AspNetCore.ResponseCompression;
using System.Net;
using WHMapper;
using WHMapper.Components;
using WHMapper.Extensions;
using WHMapper.Hubs;
using WHMapper.Services.BrowserClientIdProvider.Extension;
using Serilog;



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

// Infrastructure
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddEveAuthentication(builder.Configuration);
builder.Services.AddExternalApiClients(builder.Configuration);
builder.Services.AddMapperServices();
builder.Services.AddWHMapperTelemetry(builder.Configuration, builder.Environment.ApplicationName);

// SignalR & Blazor
builder.Services.AddSignalR();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

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

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
    });
}

var app = builder.Build();

// Startup tasks
await app.MigrateDatabaseAsync();
await app.InitializeMetricsAsync();

// Middleware pipeline
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

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

app.MapStaticAssets();
app.UseAntiforgery();
app.UseRateLimiter();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .WithStaticAssets();

app.MapHub<WHMapperNotificationHub>("/whmappernotificationhub");
app.MapGroup("/authentication").MapLoginAndLogout().RequireRateLimiting("auth");

await app.RunAsync();
