using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Cookies;
using FrontendAccountManagement.Web.Sessions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using EPR.Common.Authorization.Extensions;
using FrontendAccountManagement.Web.Middleware;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using StackExchange.Redis;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Services;

namespace FrontendAccountManagement.Web.Extensions;

public static class ServiceProviderExtension
{
    public static IServiceCollection RegisterWebComponents(this IServiceCollection services, IConfiguration configuration)
    {
        SetTempDataCookieOptions(services, configuration);
        ConfigureOptions(services, configuration);
        ConfigureLocalization(services);
        ConfigureAuthentication(services, configuration);
        ConfigureAuthorization(services, configuration);
        ConfigureSession(services, configuration);
        RegisterServices(services);
        RegisterMiddleware(services);
        
        return services;
    }

    private static void RegisterMiddleware(IServiceCollection services)
    {
        services.AddScoped<UserDataCheckerMiddleware>();
    }

    public static IServiceCollection ConfigureMsalDistributedTokenOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddApplicationInsights());
        var buildLogger = loggerFactory.CreateLogger<Program>();

        services.Configure<MsalDistributedTokenCacheAdapterOptions>(options =>
        {
            options.DisableL1Cache = configuration.GetValue("MsalOptions:DisableL1Cache", true);
            options.SlidingExpiration = TimeSpan.FromMinutes(configuration.GetValue("MsalOptions:L2SlidingExpiration", 20));

            options.OnL2CacheFailure = exception =>
            {
                if (exception is RedisConnectionException)
                {
                    buildLogger.LogError(exception, "L2 Cache Failure Redis connection exception: {message}", exception.Message);
                    return true;
                }

                buildLogger.LogError(exception, "L2 Cache Failure: {message}", exception.Message);
                return false;
            };
        });
        return services;
    }
    
    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EprCookieOptions>(configuration.GetSection(EprCookieOptions.ConfigSection));
        services.Configure<AnalyticsOptions>(configuration.GetSection(AnalyticsOptions.ConfigSection));
        services.Configure<PhaseBannerOptions>(configuration.GetSection(PhaseBannerOptions.ConfigSection));
        services.Configure<ExternalUrlsOptions>(configuration.GetSection(ExternalUrlsOptions.ConfigSection));
        services.Configure<EmailAddressOptions>(configuration.GetSection(EmailAddressOptions.ConfigSection));
        services.Configure<SiteDateOptions>(configuration.GetSection(SiteDateOptions.ConfigSection));
        services.Configure<ServiceSettingsOptions>(configuration.GetSection(ServiceSettingsOptions.ConfigSection));
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICookieService, CookieService>();
        services.AddScoped<ISessionManager<JourneySession>, SessionManager<JourneySession>>();
        services.AddTransient<IDateTimeProvider, SystemDateTimeProvider>();
    }

    private static void SetTempDataCookieOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CookieTempDataProviderOptions>(options => 
        {
            options.Cookie.Name = configuration.GetValue<string>("CookieOptions:TempDataCookie");
            options.Cookie.Path = configuration.GetValue<string>("PATH_BASE");
        });
    }

    private static void ConfigureLocalization(IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources")
            .Configure<RequestLocalizationOptions>(options =>
            {
                var cultureList = new[] { Language.English, Language.Welsh };
                options.SetDefaultCulture(Language.English);
                options.AddSupportedCultures(cultureList);
                options.AddSupportedUICultures(cultureList);
                options.RequestCultureProviders = new IRequestCultureProvider[]
                {
                    new SessionRequestCultureProvider()
                };
            });
    }

    private static void ConfigureSession(IServiceCollection services, IConfiguration configuration)
    {
        var useLocalSession = configuration.GetValue<bool>("UseLocalSession");

        if (!useLocalSession)
        {
            var redisConnection = configuration.GetConnectionString("REDIS_CONNECTION");

            services.AddDataProtection()
                .SetApplicationName("EprProducers")
                .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnection), "DataProtection-Keys");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = configuration.GetValue<string>("RedisInstanceName");
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSession(options =>
        {
            options.Cookie.Name = configuration.GetValue<string>("CookieOptions:SessionCookieName");
            options.IdleTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("SessionIdleTimeOutMinutes"));
            options.Cookie.IsEssential = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.Path = "/";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }
    
    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                configuration.GetSection("AzureAdB2C").Bind(options);
                options.ErrorPath = "/error";
                options.ClaimActions.Add(new CorrelationClaimAction());
            }, options =>
            {
                options.Cookie.Name = configuration.GetValue<string>("CookieOptions:AuthenticationCookieName");
                options.ExpireTimeSpan = TimeSpan.FromMinutes(configuration.GetValue<int>("CookieOptions:AuthenticationExpiryInMinutes"));
                options.SlidingExpiration = true;
                options.Cookie.Path = "/";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .EnableTokenAcquisitionToCallDownstreamApi(new string[] {configuration.GetValue<string>("FacadeAPI:DownstreamScope")})
            .AddDistributedTokenCaches();
    }

    private static void ConfigureAuthorization(IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterPolicy<JourneySession>(configuration);
    }
}
