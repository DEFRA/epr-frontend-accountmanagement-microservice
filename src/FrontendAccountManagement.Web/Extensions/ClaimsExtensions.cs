using EPR.Common.Authorization.Models;
using System.Security.Claims;
using System.Text.Json;
using EPR.Common.Authorization.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FrontendAccountManagement.Web.Extensions;

public static class ClaimsExtensions
{
    public static UserData GetUserData(this ClaimsPrincipal claimsPrincipal)
    {
        var userDataClaim = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.UserData);

        return JsonSerializer.Deserialize<UserData>(userDataClaim.Value);
    }
    
    public static UserData? TryGetUserData(this ClaimsPrincipal claimsPrincipal, ILogger logger)
    {
        try
        {
            return claimsPrincipal.GetUserData();
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while retrieving user data");
            return null;
        }
    }
    
    public static async Task UpdateUserDataClaimsAndSignInAsync(HttpContext httpContext, UserData userData)
    {
        var claimsIdentity = httpContext.User.Identity as ClaimsIdentity;
        var claim = claimsIdentity?.FindFirst(ClaimTypes.UserData);
        if (claim != null)
        {
            claimsIdentity?.RemoveClaim(claim);
        }

        var claims = new List<Claim> { new(ClaimTypes.UserData, JsonSerializer.Serialize(userData)) };
        claimsIdentity = new ClaimsIdentity(httpContext.User.Identity, claims);
        var principal = new ClaimsPrincipal(claimsIdentity);
        var properties = httpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);

        // We need to set the user data in the http context here to ensure it is accessible in this request
        httpContext.User.AddOrUpdateUserData(userData);
    }
}