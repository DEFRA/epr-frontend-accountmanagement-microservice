using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Localization;

namespace FrontendAccountManagement.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class RolePermissionExtensions
{
    public static LocalizedHtmlString GetPermissionDescription(this string roleKey, IViewLocalizer localizer)
    {
        if (string.IsNullOrEmpty(roleKey))
            return localizer["ManageOrganisation.TabTeam.Permissions.ReadOnly"];

        var lowerKey = roleKey.ToLowerInvariant();

        if (lowerKey.Contains("approvedperson"))
        {
            return localizer["ManageOrganisation.TabTeam.Permissions.ApprovedPerson"];
            // "Manage team, submit registration and accreditation (Approved Person)"
        }

        if (lowerKey.Contains("standarduser"))
        {
            return localizer["ManageOrganisation.TabTeam.Permissions.StandardUser"];
            // "Submit registration and accreditation (Standard User)"
        }

        if (lowerKey.Contains(".basicuser"))
        {
            return localizer["ManageOrganisation.TabTeam.Permissions.BasicUser"];
            // "Read only access (Basic User)"
        }

        return localizer["ManageOrganisation.TabTeam.Permissions.ReadOnly"];
    }
}