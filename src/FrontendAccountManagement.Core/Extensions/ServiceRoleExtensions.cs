using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceRoleExtensions
{
    public static string GetRoleName(this string roleKey)
    {
        return roleKey switch
        {
            "Re-Ex.ApprovedPerson" => "Approved Person",
            "Re-Ex.DelegatedPerson" => "Delegated Person",
            "Re-Ex.BasicUser" => "Basic User",
            "Re-Ex.AdminUser" => "Admin User",
            "Re-Ex.StandardUser" => "Standard User",
            
            _ => roleKey 
        };
    }
}