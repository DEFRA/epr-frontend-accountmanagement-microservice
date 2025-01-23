using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Core.Sessions
{
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
        public string? OrganisationType { get; set; }
        public bool IsUpdateCompanyAddress { get; set; }
        public bool IsUpdateCompanyName { get; set; }

    }
}