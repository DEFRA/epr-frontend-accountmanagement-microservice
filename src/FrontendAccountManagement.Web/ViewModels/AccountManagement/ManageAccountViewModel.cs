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
    }
}