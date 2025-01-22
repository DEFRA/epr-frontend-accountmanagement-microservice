using EPR.Common.Authorization.Interfaces;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions.Interfaces;

namespace FrontendAccountManagement.Core.Sessions
{
    public class AccountManagementSession : IHasUserData, IJourneySession
    {
        public UserData UserData { get; set; } = new();
        public List<string> Journey { get; set; } = new();

        public string? OrganisationName { get; set; }

        public string InviteeEmailAddress { get; set; } = default!;
        
        public string RoleKey { get; set; } = default!;
        
        public RemoveUserJourneyModel? RemoveUserJourney { get; set; }
        
        public AddUserJourneyModel? AddUserJourney { get; set; }

        public EndpointResponseStatus? RemoveUserStatus { get; set; }
        
        public EndpointResponseStatus? AddUserStatus { get; set; }
        public CompaniesHouseSession CompaniesHouseSession { get; set; } = new();

    }
}