using FrontendAccountManagement.Core.Configuration;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Extensions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.HealthChecks;
using FrontendAccountManagement.Web.Middleware;
using FrontendAccountManagement.Web.Utilities;
using FrontendAccountManagement.Web.Utilities.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
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
    .AddAutoMapper(typeof(Program))
    .AddAntiforgery(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Name = builder.Configuration.GetValue<string>("CookieOptions:AntiForgeryCookieName");
    })
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

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services
    .Configure<DeploymentRoleOptions>(options =>
    {
        options.DeploymentRole = builder.Configuration.GetValue<string>(DeploymentRoleOptions.ConfigSection);
    })
    .ConfigureAutoMapper();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IAuthorizationHandler,EmployeeOrBasicAdminHandler>();
builder.Services.AddTransient<IClaimsExtensionsWrapper, ClaimsExtensionsWrapper>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("IsEmployeeOrBasicAdmin", policy => 
    policy.Requirements.Add(
        new EmployeeOrBasicAdminRequirement(
            ServiceRole.Basic,
            PersonRole.Employee,
            PersonRole.Admin 
            )));

builder.Services
    .Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
        options.ForwardedHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:ForwardedHostHeaderName");
        options.OriginalHostHeaderName = builder.Configuration.GetValue<string>("ForwardedHeaders:OriginalHostHeaderName");
        options.AllowedHosts = builder.Configuration.GetValue<string>("ForwardedHeaders:AllowedHosts").Split(";");
    })
    .Configure<FacadeApiConfiguration>(builder.Configuration.GetSection(FacadeApiConfiguration.SectionName));

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

app.UseMiddleware<SecurityHeaderMiddleware>();
app.UseCookiePolicy();
app.UseSession();
app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRequestLocalization();
app.UseMiddleware<UserDataCheckerMiddleware>();
app.UseMiddleware<JourneyAccessCheckerMiddleware>();
app.UseMiddleware<AnalyticsCookieMiddleware>();

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
