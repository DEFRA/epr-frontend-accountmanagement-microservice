using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.Utilities.Interfaces;

namespace FrontendAccountManagement.Web.Utilities;

/// <summary>
/// Class to wrapper the claims extension of updating claims and
/// signing in. This simplifies unit testing
/// </summary>
public class ClaimsExtensionsWrapper : IClaimsExtensionsWrapper
{
    private readonly HttpContext _httpContext;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpContextAccessor">Interface of the HttpContextAccessor
    /// that is DI'd into the class</param>
    public ClaimsExtensionsWrapper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;    
    }

    /// <summary>
    /// Async function that Wraps the ClaimsExtensions.UpdateUserDataClaimsAndSignInAsync
    /// method
    /// </summary>
    /// <param name="userData">The latest UserData data to update the ClaimsIdentity with</param>
    /// <returns>Async task</returns>
    public async Task UpdateUserDataClaimsAndSignInAsync(UserData userData)
    {
        await ClaimsExtensions.UpdateUserDataClaimsAndSignInAsync(_httpContext, userData);
    }
}
