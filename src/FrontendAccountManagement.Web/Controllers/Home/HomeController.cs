using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FrontendAccountManagement.Web.Controllers.Home;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly EprCookieOptions _cookieOptions;

    public HomeController(ILogger<HomeController> logger, IOptions<EprCookieOptions> cookieOptions)
    {
        _logger = logger;
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
}