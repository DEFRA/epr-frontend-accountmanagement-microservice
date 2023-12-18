using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Cookies;
using Microsoft.Extensions.Options;

namespace FrontendAccountManagement.Web.Middleware;

public class AnalyticsCookieMiddleware
{
    private readonly RequestDelegate _next;

    public AnalyticsCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        ICookieService cookieService,
        IOptions<AnalyticsOptions> googleAnalyticsOptions)
    {
        httpContext.Items[ContextKeys.UseGoogleAnalyticsCookieKey] = cookieService.HasUserAcceptedCookies(httpContext.Request.Cookies);
        httpContext.Items[ContextKeys.TagManagerContainerIdKey] = googleAnalyticsOptions.Value.TagManagerContainerId;

        await _next(httpContext);
    }
}
