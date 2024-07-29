using EPR.Common.Authorization.Extensions;
using EPR.Common.Authorization.Sessions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Attributes;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace FrontendAccountManagement.Web.Middleware;

public class JourneyAccessCheckerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ExternalUrlsOptions _urlOptions;

    public JourneyAccessCheckerMiddleware(
        RequestDelegate next,
        IOptions<ExternalUrlsOptions> urlOptions,
        ILogger<JourneyAccessCheckerMiddleware> logger)
    {
        _next = next;
        _urlOptions = urlOptions.Value;
    }

    public async Task Invoke(
        HttpContext httpContext,
        ISessionManager<JourneySession> sessionManager)
    {
        if (httpContext.User.Identity is { IsAuthenticated: true } && httpContext.User.GetUserData() is null)
        {
            httpContext.Response.Redirect(_urlOptions.FrontEndCreationBaseUrl);
            return;
        }
        
        var endpoint = httpContext.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<JourneyAccessAttribute>();

        if (attribute != null)
        {
            string? pageToRedirect = null;

            if (attribute.JourneyType == JourneyName.ManagePermissionsStart ||
                attribute.JourneyType == JourneyName.ManagePermissions)
            {
                var id = ParseRouteId(httpContext);

                if (id == null)
                {
                    pageToRedirect = PagePath.ManageAccount;
                }

                if (attribute.JourneyType == JourneyName.ManagePermissions)
                {
                    var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);
                    var permissionManagementSession = sessionValue?.PermissionManagementSession;

                    var permissionManagementSessionItem = permissionManagementSession?.Items.FirstOrDefault(x => x.Id == id);

                    if (permissionManagementSessionItem == null || permissionManagementSessionItem.Journey.Count == 0)
                    {
                        pageToRedirect = PagePath.ManageAccount;
                    }
                    else if (!permissionManagementSessionItem.Journey.Contains($"{attribute.PagePath}/{id}"))
                    {
                        pageToRedirect = permissionManagementSessionItem.Journey.Last();
                    }
                }
            }
            else
            {
                var sessionValue = await sessionManager.GetSessionAsync(httpContext.Session);
                var accountManagementSessionValue = sessionValue?.AccountManagementSession;

                if (accountManagementSessionValue == null || accountManagementSessionValue.Journey.Count == 0)
                {
                    pageToRedirect = PagePath.ManageAccount;
                }
                else if (!accountManagementSessionValue.Journey.Contains(attribute.PagePath))
                {
                    pageToRedirect = accountManagementSessionValue.Journey.Last();
                }
            }

            if (!string.IsNullOrEmpty(pageToRedirect))
            {
                httpContext.Response.Redirect($"{httpContext.Request.PathBase}/{pageToRedirect}");
                return;
            }
        }

        await _next(httpContext);
    }

    private static Guid? ParseRouteId(HttpContext context)
    {
        var id = context.GetRouteData().Values["id"];

        if (!Guid.TryParse(id?.ToString(), out var identifier))
        {
            return null;
        }

        return identifier;
    }
}
