using FrontendAccountManagement.Web.Constants;
using Microsoft.AspNetCore.Localization;

namespace FrontendAccountManagement.Web.Sessions;

public class SessionRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        await Task.Yield();
        var culture = httpContext.Session.Get(Language.SessionLanguageKey) == null ? Language.English : httpContext.Session.GetString(Language.SessionLanguageKey);
        return new ProviderCultureResult(culture);
    }
}