using EPR.Common.Authorization.Models;
using System.Security.Claims;
using System.Text.Json;
using EPR.Common.Authorization.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FrontendAccountManagement.Web.Extensions;

public static class ClaimsExtensions
{
    public static async Task UpdateUserDataClaimsAndSignInAsync(HttpContext httpContext, UserData userData)
    {
        var claimsIdentity = httpContext.User.Identity as ClaimsIdentity;
        var claim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData);
        
        // there's a bug in the epr-common code where the same
        // claim gets added more than once. This ensures it's
        // tidied up correctly
        while (claim != null)
        {
            if (claim != null)
            {
                claimsIdentity?.RemoveClaim(claim);
                claim = claim = claimsIdentity?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData);
            }
        }

        var claims = new List<Claim>
        { 
            new(ClaimTypes.UserData, JsonSerializer.Serialize(userData))
        };

        claimsIdentity = new ClaimsIdentity(httpContext.User.Identity, claims);
        var principal = new ClaimsPrincipal(claimsIdentity);
        var properties = httpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        // We need to set the user data in the http context here to ensure it is accessible in this request
        httpContext.User.AddOrUpdateUserData(userData);
    }
}