using EPR.Common.Authorization.Models;
using System.Security.Claims;
using System.Text.Json;

namespace FrontendAccountManagement.Core.Extensions;

public static class ClaimsPrincipalExtension
{
    public static Guid? GetOrganisationId(this ClaimsPrincipal principal)
    {
        return GetUserData(principal)?.Organisations[0].Id;
    }

    public static bool IsApprovedPerson(this ClaimsPrincipal principal)
    {
        return CheckRole(principal, "Approved Person");
    }

    public static bool IsDelegatedPerson(this ClaimsPrincipal principal)
    {
        return CheckRole(principal, "Delegated Person");
    }

    private static bool CheckRole(ClaimsPrincipal principal, string role)
    {
        return GetUserData(principal)?.ServiceRole == role;
    }

    private static UserData? GetUserData(ClaimsPrincipal principal)
    {
        var userDataClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData);
        if (userDataClaim == null)
        {
            return null;
        }
        return JsonSerializer.Deserialize<UserData>(userDataClaim.Value);
    }
}
