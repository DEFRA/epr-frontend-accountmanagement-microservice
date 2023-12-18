using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FrontendAccountManagement.Web.Controllers.Cookies;

[AllowAnonymous]
public class CookiesController : Controller
{
    private readonly ICookieService _cookieService;
    private readonly EprCookieOptions _eprCookieOptions;
    private readonly AnalyticsOptions _googleAnalyticsOptions;

    public CookiesController(
        ICookieService cookieService,
        IOptions<EprCookieOptions> eprCookieOptions,
        IOptions<AnalyticsOptions> googleAnalyticsOptions)
    {
        _cookieService = cookieService;
        _eprCookieOptions = eprCookieOptions.Value;
        _googleAnalyticsOptions = googleAnalyticsOptions.Value;
    }

    [HttpPost]
    [Route(PagePath.UpdateCookieAcceptance)]
    public LocalRedirectResult UpdateAcceptance(string returnUrl, string cookies)
    {
        _cookieService.SetCookieAcceptance(cookies == CookieAcceptance.Accept, Request.Cookies, Response.Cookies);
        TempData[CookieAcceptance.CookieAcknowledgement] = cookies;

        return LocalRedirect(returnUrl);
    }

    [HttpPost]
    [Route(PagePath.AcknowledgeCookieAcceptance)]
    public LocalRedirectResult AcknowledgeAcceptance(string returnUrl)
    {
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.HomePath();
        }

        return LocalRedirect(returnUrl);
    }
}