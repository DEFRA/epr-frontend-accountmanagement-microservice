using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class TeamMemberPermissionsViewModel
    {
        public List<ServiceRole>? ServiceRoles { get; set; } = new();

        [Required(ErrorMessage = "SelectWhatYouWantThemToDo")]
        public string? SelectedUserRole { get; set; }

        public string? SavedUserRole { get; set; }
        public bool IsStandardUser { get; set; }
        public Guid OrganisationId { get; set; }
    }
}