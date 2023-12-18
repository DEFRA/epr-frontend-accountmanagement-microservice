using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.Extensions;
[ExcludeFromCodeCoverage]
public static class UrlHelperExtension
{
    public static string HomePath(this IUrlHelper url)
    {
        return url.Action("ManageAccount", "AccountManagement");
    }
    
    public static string CurrentPath(this IUrlHelper url)
    {
        return url.Action(null, "AccountManagement");
    }
}