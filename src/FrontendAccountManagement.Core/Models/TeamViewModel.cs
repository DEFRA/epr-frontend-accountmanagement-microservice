using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models;

[ExcludeFromCodeCoverage]
public class TeamViewModel
{
    public Guid OrganisationId { get; set; }

    public string? OrganisationName { get; set; }

    public string? OrganisationNumber { get; set; }

    public Uri AddNewUser { get; set; }

    public string AboutRolesAndPermissions { get; set; }

    public List<string> UserServiceRoles { get; set; }

    public Guid? OrganisationExternalId { get; set; }

    public List<TeamMembersResponseModel> TeamMembers { get; set; }
}