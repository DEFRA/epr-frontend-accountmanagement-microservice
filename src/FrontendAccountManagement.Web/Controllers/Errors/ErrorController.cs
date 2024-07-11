using System.Net;
using FrontendAccountManagement.Web.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountManagement.Web.Controllers.Errors;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route(PagePath.Error)]
    public ViewResult Error(int? statusCode)
    {
        var errorView = statusCode == (int?)HttpStatusCode.NotFound ? "PageNotFound" : "Error";
        return View(errorView);
    }
}
