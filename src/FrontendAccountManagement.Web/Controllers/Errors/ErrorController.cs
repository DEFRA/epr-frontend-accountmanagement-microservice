using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAccountManagement.Web.Controllers.Errors;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("{statusCode:int}")]
    public ViewResult Index(int? statusCode)
    {
        Response.StatusCode = statusCode.HasValue ? statusCode.Value : StatusCodes.Status500InternalServerError;

        if (Response.StatusCode == StatusCodes.Status404NotFound)
        {
            return View("PageNotFound");
        }
        else
        {
            return View("Error");
        }
    }
}