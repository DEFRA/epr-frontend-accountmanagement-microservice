using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Core.Models;

[ExcludeFromCodeCoverage]
public class ReExRemoveUserJourneyModel : RemoveUserJourneyModel
{
    public Guid OrganisationId { get; set; }
    public string Role { get; set; }
}