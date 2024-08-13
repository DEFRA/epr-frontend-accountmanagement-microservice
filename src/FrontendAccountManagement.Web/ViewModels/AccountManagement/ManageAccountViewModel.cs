
using FrontendAccountManagement.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class ManageAccountViewModel
    {
        public EndpointResponseStatus? UserRemovedStatus { get; set; }

        public string? RemovedUsersName { get; set; }

        public EndpointResponseStatus? InviteStatus { get; set; }

        public string? InvitedUserEmail { get; set; }

        public string? PersonUpdated { get; set; }

        public string? CompanyName { get; set; }

        public string? OrganisationAddress { get; set; }

        public string? UserName { get; set; }

        public string? JobTitle { get; set; }

        public string? Telephone { get; set; }

        public string? EnrolmentStatus { get; set; }

        public string? ServiceRoleKey { get; set; }

        public string? OrganisationType { get; set; }

        public bool? HasPermissionToChangeCompany { get; set; } = false;

        public bool? IsBasicUser { get; set; } = false;

        public bool? IsChangeRequestPending { get; set; } = false;

        public bool? IsAdmin { get; set; }
    }
}