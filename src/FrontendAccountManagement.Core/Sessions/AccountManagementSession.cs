using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;

public class AccountManagementSession
{
    public List<string> Journey { get; set; } = new();

    public string? OrganisationName { get; set; }

    public string InviteeEmailAddress { get; set; } = default!;
    public string RoleKey { get; set; } = default!;
    public RemoveUserJourneyModel? RemoveUserJourney { get; set; }
    public AddUserJourneyModel? AddUserJourney { get; set; }

    public EndpointResponseStatus? RemoveUserStatus { get; set; }
    public EndpointResponseStatus? AddUserStatus { get; set; }
    public Nation? UkNation { get; set; }
    public string? OrganisationType { get; set; }
    public Guid? OrganisationId { get; set; }
    public Address? BusinessAddress { get; set; }
    public List<Address?> AddressesForPostcode { get; set; } = new();
    public bool IsUpdateCompanyAddress { get; set; }
    public bool IsUpdateCompanyName { get; set; }

}