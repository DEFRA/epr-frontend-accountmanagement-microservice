using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.Controllers.Home;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly EprCookieOptions _cookieOptions;

    public HomeController(IOptions<EprCookieOptions> cookieOptions)
    {
        _cookieOptions = cookieOptions.Value;
    }

    [AllowAnonymous]
    [Route(PagePath.SignedOut)]
    public IActionResult SignedOut()
    {
        HttpContext.Session.Clear();
        HttpContext.Response.Cookies.Delete(_cookieOptions.SessionCookieName);
        return View();
    }

    [ExcludeFromCodeCoverage(Justification = "For SessionTime Out")]
    [Route(PagePath.TimeoutSignedOut)]
    public IActionResult TimeoutSignedOut()
    {
        HttpContext.Session.Clear();
        return View();
    }
    [ExcludeFromCodeCoverage(Justification = "For SessionTime Out")]
    public IActionResult SessionTimeoutModal()
    {
        return PartialView("_TimeoutSessionWarning");
    }
}