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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using WHMapper.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
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
using System.Linq;
using WHMapper.Services.EveMapper.AuthorizationPolicies;
using WHMapper.Repositories.WHNotes;

namespace WHMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.
            builder.Services.AddDbContextFactory<WHMapperContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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


            builder.Services.AddSingleton<IConfiguration>(provider => builder.Configuration);
            builder.Services.AddHttpClient();

            builder.Services.AddScoped<TokenProvider>();

            builder.Services.AddScoped<AuthenticationStateProvider, EveAuthenticationStateProvider>();

            builder.Services.AddScoped<IEveUserInfosServices, EveUserInfosServices>();
            builder.Services.AddScoped<IEveAPIServices, EveAPIServices>();
            builder.Services.AddScoped<ICharacterServices, CharacterServices>();
            builder.Services.AddSingleton<IAnoikServices, AnoikServices>();
            builder.Services.AddSingleton<ISDEServices, SDEServices>();
            

            #region DB Acess Repo
            builder.Services.AddScoped<IWHAdminRepository, WHAdminRepository>();
            builder.Services.AddScoped<IWHAccessRepository, WHAccessRepository>();
            builder.Services.AddScoped<IWHMapRepository, WHMapRepository>();
            builder.Services.AddScoped<IWHSystemRepository, WHSystemRepository>();
            builder.Services.AddScoped<IWHSignatureRepository, WHSignatureRepository>();
            builder.Services.AddScoped<IWHSystemLinkRepository, WHSystemLinkRepository>();
            builder.Services.AddScoped<IWHNoteRepository, WHNoteRepository>();
            builder.Services.AddScoped<IWHRouteRepository, WHRouteRepository>();
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
