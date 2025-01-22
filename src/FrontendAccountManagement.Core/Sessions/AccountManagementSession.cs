using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions.Interfaces;

namespace FrontendAccountManagement.Core.Sessions
{
    public class AccountManagementSession : IJourneySession
    {
        public List<string> Journey { get; set; } = new();

        public string? OrganisationName { get; set; }

        public string InviteeEmailAddress { get; set; } = default!;
        
        public string RoleKey { get; set; } = default!;
        
        public RemoveUserJourneyModel? RemoveUserJourney { get; set; }
        
        public AddUserJourneyModel? AddUserJourney { get; set; }

        public EndpointResponseStatus? RemoveUserStatus { get; set; }
        
        public EndpointResponseStatus? AddUserStatus { get; set; }

    }
}