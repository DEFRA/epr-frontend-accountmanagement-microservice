using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.HealthChecks;
using FrontendAccountManagement.Web.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFeatureManagement();

builder.Services
    .RegisterCoreComponents(builder.Configuration)
    .RegisterWebComponents(builder.Configuration)
    .ConfigureMsalDistributedTokenOptions(builder.Configuration);

builder.Services
    .AddAntiforgery(options => options.Cookie.Name = builder.Configuration.GetValue<string>("CookieOptions:AntiForgeryCookieName"))
    .AddControllersWithViews(options => {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    })
    .AddViewLocalization(options =>
    {
        var deploymentRole = builder.Configuration
            .GetValue<string>(DeploymentRoleOptions.ConfigSection);

        options.ResourcesPath = deploymentRole != DeploymentRoleOptions.RegulatorRoleValue 
            ? "Resources" 
            : "ResourcesRegulator";
    })
    .AddDataAnnotationsLocalization();

builder.Services.Configure<DeploymentRoleOptions>(options =>
{
    options.DeploymentRole = builder.Configuration.GetValue<string>(DeploymentRoleOptions.ConfigSection);
});

builder.Services.AddRazorPages();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
    options.ForwardedHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:ForwardedHostHeaderName");
    options.OriginalHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:OriginalHostHeaderName");
    options.AllowedHosts = builder.Configuration.GetValue<string>("ForwardedHeaders:AllowedHosts").Split(";");
});

builder.Services
    .AddApplicationInsightsTelemetry()
    .AddHealthChecks();

builder.Services.AddHsts(options =>
{
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

var app = builder.Build();

app.UsePathBase(builder.Configuration.GetValue<string>("PATH_BASE"));

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseForwardedHeaders();

// Needed when multiple web apps share an app service
var options = new SessionOptions();
options.IdleTimeout = TimeSpan.FromHours(12);
options.Cookie.Name = ".FrontendAccountManagement.Session";
options.Cookie.IsEssential = true;

app.UseMiddleware<SecurityHeaderMiddleware>();
app.UseCookiePolicy();
app.UseSession(options);
app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestLocalization();

app.UseWhen(
    context => !context.Request.Path.ToString().Equals("/admin/health"),
    appBuilder =>
    {
        // No user data causes an error when the user is anonymous, like the health check probe
        appBuilder.UseMiddleware<UserDataCheckerMiddleware>();
        appBuilder.UseMiddleware<JourneyAccessCheckerMiddleware>();
        appBuilder.UseMiddleware<AnalyticsCookieMiddleware>();
    }
);



app.MapControllerRoute(
    name: "Default",
    pattern: "{controller}/{action}/{id}",
    defaults: new { controller = "AccountManagement", action = "ManageAccount" }
);

app.MapHealthChecks(
    builder.Configuration.GetValue<string>("HealthCheckPath"),
    HealthCheckOptionBuilder.Build()).AllowAnonymous();

app.MapRazorPages();

app.Run();

namespace FrontendAccountManagement
{
    public partial class Program
    {
    }
}
