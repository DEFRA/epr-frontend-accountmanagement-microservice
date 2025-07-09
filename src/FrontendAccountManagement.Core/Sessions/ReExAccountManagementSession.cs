using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Core.Sessions;

public class ReExAccountManagementSession
{
    public List<string> Journey { get; set; } = [];

    public Guid PersonId { get; set; } = Guid.Empty;

    public Guid OrganisationId { get; set; } = Guid.Empty;

    public string? OrganisationName { get; set; }

    public string InviteeEmailAddress { get; set; } = default!;
    
    public string RoleKey { get; set; } = default!;
    
    public ReExRemoveUserJourneyModel? ReExRemoveUserJourney { get; set; }
    
    public AddUserJourneyModel? AddUserJourney { get; set; }

    public EndpointResponseStatus? RemoveUserStatus { get; set; }
    
    public EndpointResponseStatus? AddUserStatus { get; set; }
    
    public string? OrganisationType { get; set; }
    
    public bool? IsUpdateCompanyAddress { get; set; }
    
    public bool? IsUpdateCompanyName { get; set; }
    
    public Address? BusinessAddress { get; set; }
    
    public List<Address?> AddressesForPostcode { get; set; } = [];
    
    public FrontendAccountManagement.Core.Enums.Nation? UkNation { get; set; } = new();
}