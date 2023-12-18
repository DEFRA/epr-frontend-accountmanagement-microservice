using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class TeamMemberDetailsViewModel
    {
        public string Email { get; set; }

        public string SelectedUserRole { get; set; }

        public string OrganisationName { get; set; }

        public Guid OrganisationId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}