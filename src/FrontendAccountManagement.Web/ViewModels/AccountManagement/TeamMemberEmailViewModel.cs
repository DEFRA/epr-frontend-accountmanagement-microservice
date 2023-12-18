using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FrontendAccountManagement.Web.ViewModels.Attributes;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class TeamMemberEmailViewModel
    {
        [StringLength(254, ErrorMessage = "EmailTooLongErrorMessage")]
        [Required(ErrorMessage = "EnterTeamMembersEmailErrorMessage")]
        [EmailValidation("EnterTeamMembersEmailInCorrectFormatErrorMessage")]
        public string Email { get; set; }

        public string? SavedEmail { get; set; }
    }
}